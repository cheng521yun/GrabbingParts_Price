using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrabbingParts.Model;
namespace GrabbingParts.BLL.Price
{
    //public interface IPrice<T> where T: class
    //{
    //    List<T> GetPrice(string mid);
    //}
    public interface IPrice
    {
        List<PriceResult> GetPrice(string PN);
    }
    public class BasePrice
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger("WXH");
    }

}

