using Flyp_Extension_Backend.Secondary_Classes;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Security.Policy;
using System.Text;

namespace Flyp_Extension_Backend
{
    public partial class Home : Form
    {
        public Home()
        {
            InitializeComponent();
        }

        private static HttpListener _listener;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ProcessRequest(IAsyncResult result)
        {
            HttpListenerContext context = _listener.EndGetContext(result);
            HttpListenerResponse response = context.Response;
            HttpListenerRequest request = context.Request;

            _listener.BeginGetContext(new AsyncCallback(ProcessRequest), null);

            string postData;
            object jsonResponse;

            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                postData = reader.ReadToEnd();
                List<Listing> listings = new List<Listing>();
                List<FlypListing> flypListings = new List<FlypListing>();
                List<MercariListing> mercariListings = new List<MercariListing>();
                List<PoshmarkListing> poshmarkListings = new List<PoshmarkListing>();
                List<EbayListing> ebayListings = new List<EbayListing>();
                try
                {
                    // Add CORS headers
                    response.Headers.Add("Access-Control-Allow-Origin", "*"); // Allow all origins
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS"); // Allow specific HTTP methods
                    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization"); // Allow specific headers

                    if (request.HttpMethod == "OPTIONS")
                    {
                        // Handle preflight requests
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.OutputStream.Close();
                        return;
                    }

                    string[] brands;
                    switch (postData)
                    {
                        case string a when a.Contains("flyp"):
                            (flypListings, brands) = BuildFlypList(postData);
                            CreateCSV(flypListings, brands);
                            jsonResponse = new
                            {
                                success = true,
                                message = "Flyp listings processed successfully.",
                            };
                            break;
                        default:
                            jsonResponse = new
                            {
                                success = false,
                                message = "Unable to identify data.",
                            };
                            break;
                    }

                    string jsonString = JsonConvert.SerializeObject(jsonResponse);

                    // Send a 200 response
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.ContentType = "application/json";
                    byte[] buffer = Encoding.UTF8.GetBytes(jsonString);
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception)
                {
                    // Handle any exceptions and return an error response
                    jsonResponse = new
                    {
                        success = false,
                        message = "An error occured.",
                    };
                    string jsonString = JsonConvert.SerializeObject(jsonResponse);
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.ContentType = "application/json";
                    byte[] buffer = Encoding.UTF8.GetBytes(jsonString);
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    Debug.WriteLine($"Error processing request");
                }
                finally
                {
                    // Close the response stream
                    response.OutputStream.Close();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:60024/");
            _listener.Start();
            _listener.BeginGetContext(new AsyncCallback(ProcessRequest), null);
            Debug.WriteLine("Starting Server...");
        }

        private (List<FlypListing>, string[]) BuildFlypList(string postData)
        {
            string[] elements = postData.Split(" next ");
            List<FlypListing> listings = new List<FlypListing>();
            List<string> brands = new List<string>();
            foreach (string element in elements)
            {
                try
                {
                    string[] split = element.Split("|");
                    string fixed_element = split[0].Replace(",", "");
                    fixed_element = fixed_element.Replace("Åf", "'");
                    if (fixed_element != "\"" && fixed_element != "")
                    {
                        try
                        {
                            // Build listing object.
                            int titleStartIndex = fixed_element.IndexOf(" single-item-card__title") + " single-item-card__title".Length + 2;
                            int titleCharacterCount = fixed_element.IndexOf("</span>", titleStartIndex) - titleStartIndex;
                            string title = fixed_element.Substring(titleStartIndex, titleCharacterCount);

                            int imageStartIndex;
                            int imageCharacterCount;
                            string image;
                            try
                            {
                                imageStartIndex = fixed_element.IndexOf("https://flyp-tools-thumbnails.");
                                imageCharacterCount = (fixed_element.IndexOf(".jpeg") + ".jpeg".Length) - imageStartIndex;
                                image = fixed_element.Substring(imageStartIndex, imageCharacterCount);
                            }
                            catch (Exception)
                            {
                                imageStartIndex = fixed_element.IndexOf("https://flyp-lister-photos.");
                                imageCharacterCount = (fixed_element.IndexOf(".jpeg") + ".jpeg".Length) - imageStartIndex;
                                image = fixed_element.Substring(imageStartIndex, imageCharacterCount);
                                //https://flyp-lister-photos.
                            }

                            int dateStartIndex = fixed_element.IndexOf("\"ant-typography\"") + "\"ant-typography\"".Length + 1;
                            int dateCharacterCount = fixed_element.IndexOf("</span></div>", dateStartIndex) - dateStartIndex;
                            string date = fixed_element.Substring(dateStartIndex, dateCharacterCount);

                            int priceStartIndex = fixed_element.IndexOf("$");
                            int priceCharacterCount = fixed_element.IndexOf("</span></div>", priceStartIndex) - priceStartIndex;
                            string price = fixed_element.Substring(priceStartIndex, priceCharacterCount);

                            string quantity = "N/A";
                            if (fixed_element.Contains("Quantity: "))
                            {
                                int quantityStartIndex = fixed_element.IndexOf("Quantity: ") + "Quantity: ".Length;
                                int quantityCharacterCount = fixed_element.IndexOf("</span>", quantityStartIndex) - quantityStartIndex;
                                quantity = fixed_element.Substring(quantityStartIndex, quantityCharacterCount);
                            }

                            bool sold = fixed_element.Contains("Sold on ");
                            bool mercari = fixed_element.Contains("mercari");
                            bool poshmark = fixed_element.Contains("poshmark");
                            listings.Add(new(title, split[2], split[1], image, date, price, quantity, sold, mercari, poshmark));
                        }
                        catch (Exception)
                        {
                            Debug.WriteLine("Failed to create listing.");
                        }
                        brands.Add(split[2]);
                        Debug.WriteLine("Created listing.");
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Invalid data.");
                }
            }
            return (listings, brands.ToArray());
        }

        private void CreateCSV(List<FlypListing> listings, string[] brands)
        {
            // Write all data to a CSV file.
            foreach (string brand in brands)
            {
                StringBuilder csv = new StringBuilder();
                try
                {
                    csv.Append(File.ReadAllText($"out/{brand}.csv"));
                }
                catch { csv.AppendLine("Title, Brand, Ebay URL, Image, Date Added, Price, Quantity, Sold, Mercari, Poshmark"); }
                foreach (FlypListing listing in listings)
                {
                    if (listing != null && listing.brand == brand)
                    {
                        string newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", listing.title, listing.brand, listing.ebayLink, listing.image, listing.date, listing.price, listing.quantity, listing.sold, listing.mercari, listing.poshmark);
                        if (!csv.ToString().Contains(newLine))
                        {
                            csv.AppendLine(newLine);
                        }
                    }
                }
                try
                {
                    File.WriteAllText($"out/{brand}.csv", csv.ToString());
                    Debug.WriteLine("Succesfully wrote CSV.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to write CSV.");
                }
            }
        }
    }
}
