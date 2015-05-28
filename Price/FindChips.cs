using GrabbingParts.BLL.Common;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using GrabbingParts.Model;
namespace GrabbingParts.BLL.Price
{
    public class Findchips : BasePrice, IPrice
    {
        public static string url = "http://www.findchips.com/search/{0}";//AD9523-1BCPZ
        public HtmlDocument doc;
        public List<PriceResult> priceList = new List<PriceResult>();

        public List<PriceResult> GetPrice(string PN)
        {
            try
            {
                url = string.Format(url, PN);

                //need to fixed the page issue~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~`
                HtmlWeb htmlWeb = new HtmlWeb();
                doc = Retry.Do(() => htmlWeb.Load(url), TimeSpan.FromSeconds(30), 1);
                doc = Common.RemoveRedundancyCode(doc);
                GetResult();
            }
            catch
            {
                log.ErrorFormat("Error URL:{0}", url);
            }
            return this.priceList;
        }

        public void GetResult()
        {
            string baiscInf = "//div[@class='distributor-results']";
            HtmlNodeCollection infNodes = doc.DocumentNode.SelectNodes(baiscInf);

            if (infNodes == null)
                return;
            for (int i = 0; i < infNodes.Count(); i++)
            {
                GetResultbySubsupplier(infNodes[i]);
            }
        }

        public void GetResultbySubsupplier(HtmlNode supplierNode)
        {
            HtmlNode PNNode = supplierNode.SelectSingleNode("h3");
            if (PNNode == null)
                return;

            string subsupplier = PNNode.InnerText;

            if (PNNode != null)
            {
                HtmlNodeCollection partsNode = supplierNode.SelectNodes("table/tbody/tr");
                foreach (HtmlNode hn in partsNode)
                {
                    GetSinglePart(hn, subsupplier);
                }
            }

        }
        public void GetSinglePart(HtmlNode singleNode,string subsupplier)
        {            
            PriceResult price = new PriceResult();

            HtmlNode infNode = singleNode;
            HtmlNodeCollection partNode = infNode.SelectNodes("td");
            if (partNode == null)
                return;
            price.ManufacturerID = Common.FormatString(partNode[0].SelectSingleNode("a").InnerText);
            price.Manufacturer =Common.FormatString(partNode[1].InnerText);
            price.Description = Common.FormatString(partNode[2].InnerText);

            price.Stock = Common.FormatString(partNode[3].InnerText);
            
            price.Supplier = "findchips";
            price.SubSupplier = Common.FormatString( subsupplier);

            price.Prices = GetPriceTable(partNode[4], price);

            this.priceList.Add(price);
        }


        public List<Model.Price> GetPriceTable(HtmlNode infNode, Model.PriceResult price)
        {
            List<Model.Price> priceModels = new List<Model.Price>();

            HtmlNodeCollection priceNodes = infNode.SelectNodes("ul/li");
            if (priceNodes == null)
                return null;

            int count = priceNodes.Count();
            if (priceNodes[count - 1].InnerText.Contains("了解更多") || priceNodes[count - 1].InnerText.Contains("See More"))
                count = count - 1;
            for (int i = 0; i < count; i++)
            {
                Model.Price priceModel = new Model.Price();
                HtmlNodeCollection currentNodes = priceNodes[i].SelectNodes("span");

                if (i + 1 == count )
                {
                    priceModel.MaxQuantity = int.MaxValue;
                }
                else
                {
                    HtmlNodeCollection nextNodes = priceNodes[i + 1].SelectNodes("span");

                    if (nextNodes == null)
                        break;
                    priceModel.MaxQuantity = int.Parse(Common.FormatNumber(nextNodes[0].InnerText));
                }
                
                priceModel.MinQuantity = int.Parse(Common.FormatNumber(currentNodes[0].InnerText));
                if (currentNodes[1] != null)
                {
                    price.MoneyType = Common.FormatCurrency(HttpUtility.HtmlDecode(currentNodes[1].InnerText).Substring(0, 1));
                    priceModel.UnitPrice = double.Parse(Common.FormatNumber(currentNodes[1].InnerText));

                }

                priceModels.Add(priceModel);
            }

            return priceModels;
        }
    }
}
