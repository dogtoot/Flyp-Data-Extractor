using Flyp_Extension_Backend.Secondary_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyp_Extension_Backend
{
    internal class Listing
    {
        FlypListing? FlypListing;
        PoshmarkListing? PoshmarkListing;
        EbayListing? EbayListing;
        MercariListing? MercariListing;

        public Listing(FlypListing? flypListing, PoshmarkListing? poshmarkListing, EbayListing? ebayListing, MercariListing? mercariListing)
        {
            FlypListing = flypListing;
            PoshmarkListing = poshmarkListing;
            EbayListing = ebayListing;
            MercariListing = mercariListing;
        }
    }
}
