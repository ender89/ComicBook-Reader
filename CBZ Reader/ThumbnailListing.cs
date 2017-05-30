using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CBZ_Reader
{
    class ThumbnailListing
    {
        public string Number { get; set; }
        public Image Thumbnail { get; set; }
        
        public ThumbnailListing(string number, Image thumbnail)
        {
            this.Number = number;
            this.Thumbnail = thumbnail;
        }
    }
}
