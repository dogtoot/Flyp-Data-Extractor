using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyp_Extension_Backend
{
    internal class Listing
    {
        public string title;
        public string ebayLink;
        public string image;
        public string date;
        public string price;
        public string quantity;
        public bool sold;
        public bool mercari;
        public bool poshmark;

        public Listing(string title, string ebayLink, string image, string date, string price, string quantity, bool sold, bool mercari, bool poshmark)
        {
            this.title = title;
            this.ebayLink = ebayLink;
            this.image = image;
            this.date = date;
            this.price = price;
            this.quantity = quantity;
            this.sold = sold;
            this.mercari = mercari;
            this.poshmark = poshmark;
        }
    }
}
