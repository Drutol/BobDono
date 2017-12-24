using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class MerchandiseItem
    {
        public long Id { get; set; }

        public string ImageLink { get; set; }
        public string Notes { get; set; }
        public string Name { get; set; }
    }
}
