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
    public class Rocelec : BasePrice, IPrice
    {
        public static string url = "https://www.rocelec.com/parts/results/all/?s={0}";//M4T28-BR12SH1
        public static HtmlDocument doc;
        public List<PriceResult> priceList = new List<PriceResult>();
        public PriceResult price = new PriceResult();

        public List<PriceResult> GetPrice(string PN)
        {
            try
            {
                url = string.Format(url, PN);

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

        /// <summary>
        /// 取基本信息
        /// </summary>
        public void GetResult()
        {
            string baiscInf = "//section[@id='search-results']/table/tbody/tr";
            HtmlNodeCollection infNodes = doc.DocumentNode.SelectNodes(baiscInf);

            for (int i = 0; i < infNodes.Count(); i++)
            {
                GetSinglePart(infNodes[i]);
            }
        }

        public void GetSinglePart(HtmlNode singleNode)
        {
            PriceResult price = new PriceResult();

            HtmlNode infNode = singleNode;
            HtmlNodeCollection infoNodes = infNode.SelectNodes("td");

            price.ManufacturerID = Common.FormatString(singleNode.SelectSingleNode("th/a").InnerText);
            string Manufature = Common.FormatString(singleNode.SelectSingleNode("td[1]/p[1]").InnerText);
            price.Manufacturer = Common.FormatString(Manufature.Split(':')[1]);
            price.Description = Common.FormatString(singleNode.SelectSingleNode("td[1]/p[2]").InnerText);
            string stock = Common.FormatString(singleNode.SelectSingleNode("td[2]/p/strong").InnerText);
            price.Stock = Common.FormatString(stock.Split(':')[1]);

            price.Supplier = "rocelec";
            price.MoneyType = "USD";

            price.Prices = GetPriceTable(singleNode);

            this.priceList.Add(price);
        }
        public List<Model.Price> GetPriceTable(HtmlNode priceNode)
        {
            if (priceNode != null)
            {
                List<Model.Price> priceModels = new List<Model.Price>();
                string quantities = "td[2]/table/thead/tr/th";
                string prices = "td[2]/table/tbody/tr/td";
                HtmlNodeCollection quantitiesNodes = priceNode.SelectNodes(quantities);
                HtmlNodeCollection pricesNodes = priceNode.SelectNodes(prices);
                if (quantitiesNodes == null)
                    return null;
                int count = quantitiesNodes.Count();
                for (int i = 0; i < count; i++)
                {
                    Model.Price priceModel = new Model.Price();
                    string quantity = Common.FormatString(quantitiesNodes[i].InnerText);

                    priceModel.MinQuantity = int.Parse(quantity.Split('-')[0]);
                    if (i + 1 == count)
                        priceModel.MaxQuantity = int.MaxValue;
                    else
                        priceModel.MaxQuantity = int.Parse(quantity.Split('-')[1]);

                    priceModel.UnitPrice = double.Parse(Common.FormatString(pricesNodes[i].InnerText));
                    priceModels.Add(priceModel);
                }
                return priceModels;
            }
            return null;
        }
    }
}

