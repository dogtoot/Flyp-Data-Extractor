using System.Diagnostics;
using System.Net;
using System.Text;

namespace Flyp_Extension_Backend
{
    public partial class Form1 : Form
    {
        public Form1()
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
            HttpListenerRequest request = context.Request;

            _listener.BeginGetContext(new AsyncCallback(ProcessRequest), null);
            string postData;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                postData = reader.ReadToEnd();
                string[] elements = postData.Split(" next ");
                List<Listing> listings = new List<Listing>();
                foreach(string element in elements)
                {
                    string[] split = element.Split("|");
                    string fixed_element = split[0].Replace(",", "");
                    fixed_element = fixed_element.Replace("Åf", "'");
                    if(fixed_element != "\"" && fixed_element != "")
                    {
                        listings.Add(GetListing(fixed_element, split[1]));
                        Debug.WriteLine("Created listing.");
                    }
                }
                CreateCSV(listings);
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

        private Listing GetListing(string element, string url)
        {
            try
            {
                // Build listing object.
                int titleStartIndex = element.IndexOf(" single-item-card__title") + " single-item-card__title".Length + 2;
                int titleCharacterCount = element.IndexOf("</span>", titleStartIndex) - titleStartIndex;
                string title = element.Substring(titleStartIndex, titleCharacterCount);

                int imageStartIndex;
                int imageCharacterCount;
                string image;
                try
                {
                    imageStartIndex = element.IndexOf("https://flyp-tools-thumbnails.");
                    imageCharacterCount = (element.IndexOf(".jpeg") + ".jpeg".Length) - imageStartIndex;
                    image = element.Substring(imageStartIndex, imageCharacterCount);
                }
                catch (Exception)
                {
                    imageStartIndex = element.IndexOf("https://flyp-lister-photos.");
                    imageCharacterCount = (element.IndexOf(".jpeg") + ".jpeg".Length) - imageStartIndex;
                    image = element.Substring(imageStartIndex, imageCharacterCount);
                    //https://flyp-lister-photos.
                }

                int dateStartIndex = element.IndexOf("\"ant-typography\"") + "\"ant-typography\"".Length + 1;
                int dateCharacterCount = element.IndexOf("</span></div>", dateStartIndex) - dateStartIndex;
                string date = element.Substring(dateStartIndex, dateCharacterCount);

                int priceStartIndex = element.IndexOf("$");
                int priceCharacterCount = element.IndexOf("</span></div>", priceStartIndex) - priceStartIndex;
                string price = element.Substring(priceStartIndex, priceCharacterCount);

                string quantity = "N/A";
                if(element.Contains("Quantity: "))
                {
                    int quantityStartIndex = element.IndexOf("Quantity: ") + "Quantity: ".Length;
                    int quantityCharacterCount = element.IndexOf("</span>", quantityStartIndex) - quantityStartIndex;
                    quantity = element.Substring(quantityStartIndex, quantityCharacterCount);
                }

                bool sold = element.Contains("Sold on ");
                bool mercari = element.Contains("mercari");
                bool poshmark = element.Contains("poshmark");
                return new(title, url, image, date, price, quantity, sold, mercari, poshmark);
            }
            catch (Exception)
            {
                return null;
            }

        }

        private void CreateCSV(List<Listing> listings)
        {
            // Write all data to a CSV file.
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Title, Ebay URL, Image, Date Added, Price, Quantity, Sold, Mercari, Poshmark");
            foreach (Listing listing in listings)
            {
                if (listing != null) 
                {
                    string newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", listing.title, listing.ebayLink, listing.image, listing.date, listing.price, listing.quantity, listing.sold, listing.mercari, listing.poshmark);
                    csv.AppendLine(newLine);
                }
            }
            try
            {
                File.WriteAllText("sheet.csv", csv.ToString());
                Debug.WriteLine("Succesfully wrote CSV.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to write CSV.");
            }

        }
    }
}
