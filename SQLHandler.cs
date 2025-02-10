using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;

namespace Flyp_Extension_Backend
{
    static class SQLHandler
    {
        public static bool Insert(Object listing)
        {
            if (Exists(listing)) 
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=data\\out.db"))
                {
                    conn.Open();
                    if (listing.GetType() == typeof(FlypListing))
                    {
                        FlypListing flypListing = (FlypListing)listing;
                        var command = conn.CreateCommand();
                        command.CommandText =
                        @"
                            INSERT INTO FlypListing (title, brand, ebayLink, image, date, price, quantity, sold, mercariStatus, poshmarkStatus)
                            VALUES ($title, $brand, $ebayLink, $image, $date, $price, $quantity, $sold, $mercariStatus, $poshmarkStatus);
                        ";
                        command.Parameters.AddWithValue("$title", flypListing.title);
                        command.Parameters.AddWithValue("$brand", flypListing.brand);
                        command.Parameters.AddWithValue("$ebayLink", flypListing.ebayLink);
                        command.Parameters.AddWithValue("$image", flypListing.image);
                        command.Parameters.AddWithValue("$date", flypListing.date);
                        command.Parameters.AddWithValue("$price", flypListing.price);
                        command.Parameters.AddWithValue("$quantity", (flypListing.quantity == @"N\A" ? Int32.Parse(flypListing.quantity) : -1));
                        command.Parameters.AddWithValue("$sold", flypListing.sold ? 1 : 0);
                        command.Parameters.AddWithValue("$mercariStatus", flypListing.mercari ? 1 : 0);
                        command.Parameters.AddWithValue("$poshmarkStatus", flypListing.poshmark ? 1 : 0);

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool Exists(Object listing)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=data\\out.db"))
            {
                conn.Open();
                if (listing.GetType() == typeof(FlypListing))
                {
                    FlypListing flypListing = (FlypListing)listing;
                    var command = conn.CreateCommand();
                    command.CommandText =
                    @"
                    SELECT * FROM FlypListing WHERE image=$image
                    ";
                    command.Parameters.AddWithValue("$image", flypListing.image);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var name = reader.GetString(0);
                                return true;
                            }
                            catch 
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
