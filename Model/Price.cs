using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrabbingParts.Model
{
    public class PriceResult
    {
        public string ManufacturerID { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public string SubSupplier { get; set; }
        public string Stock { get; set; }
        public string MoneyType { get; set; }
        public string Package { get; set; }
        public string Description { get; set; }        

        public List<Price> Prices { get; set; }
    }
    public class Price
    {
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public double UnitPrice { get; set; }
    }
}
