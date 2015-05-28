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
    public class Onlinecomponents : BasePrice, IPrice
    {
        public static string url = "http://ex-en.alliedelec.com/search/results.aspx?term={0}";//MV10-10FBX
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
            string baiscInf = "//table[@id='myTable']/tbody/tr";
            HtmlNodeCollection infNodes = doc.DocumentNode.SelectNodes(baiscInf);

            if (infNodes != null)
                GetParts(infNodes);
            else
                GetExactPart();
        }
        public void GetParts(HtmlNodeCollection infNodes)
        {            
            if (infNodes == null)
                return;
            for (int i = 0; i < infNodes.Count(); i++)
            {
                if (i != 0)
                    GetSinglePart(infNodes[i]);
            }
        }

        public void GetSinglePart(HtmlNode singleNode)
        {            
            PriceResult price = new PriceResult();

            HtmlNode infNode = singleNode;
            HtmlNodeCollection infoNodes = infNode.SelectNodes("td");

            price.Stock = Common.FormatString(HandleStock(infoNodes[6]));
            price.Manufacturer = Common.FormatString(infoNodes[3].InnerText);
            price.ManufacturerID = Common.FormatString(infoNodes[2].InnerText);
            //price.Description = Common.FormatString(infoNodes[3].InnerText);

            price.Supplier = "onlinecomponents";
            price.MoneyType = "USD";
            price.Prices = GetPriceTable(infoNodes[7]);

            this.priceList.Add(price);
        }


        public List<Model.Price> GetPriceTable(HtmlNode priceNode)
        {
            if (priceNode != null)
            {
                List<Model.Price> priceModels = new List<Model.Price>();
                string sectionPrices = "table[@class='PriceListKeyword']/tr";
                HtmlNodeCollection pricesNodes = priceNode.SelectNodes(sectionPrices);
                int count = pricesNodes.Count();
                if (count == 1)
                    return null;

                for (int i = 0; i < count; i++)
                {
                    Model.Price priceModel = new Model.Price();
                    HtmlNodeCollection currentNodes = pricesNodes[i].SelectNodes("td");
                    if (currentNodes.Count == 1)
                        break;

                    string quantities = currentNodes[0].InnerText.Replace(":", "");

                    if (i + 1 == count)
                    {
                        priceModel.MaxQuantity = int.MaxValue;
                    }
                    else
                    {

                        priceModel.MaxQuantity = int.Parse(Common.FormatString(quantities.Split('-')[1]));
                    }

                    priceModel.MinQuantity = int.Parse(Common.FormatString(quantities.Split('-')[0]));
                    priceModel.UnitPrice = double.Parse(Common.FormatString(currentNodes[1].InnerText));

                    priceModels.Add(priceModel);
                }
                return priceModels;
            }
            return null;
        }
        #region 取单个零件，具体页面
        public void GetExactPart()
        {
            string baiscInf = "//table[@id='TablePartDetailsDiv2']/tr/td/table/tr";
            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes(baiscInf);

            if (infoNodes == null)
                return;

            PriceResult price = new PriceResult();

            string stockpath = "//table[@class='tbAvailability']/tr/td/table/tr[1]/td[2]";
            HtmlNode stockNode = doc.DocumentNode.SelectSingleNode(stockpath);

            price.Stock = Common.FormatString(HandleStock(stockNode));
            price.Manufacturer = Common.FormatString(infoNodes[0].SelectSingleNode("td[2]").InnerText);
            price.ManufacturerID = Common.FormatString(infoNodes[1].SelectSingleNode("td[2]").InnerText);
            
            price.Description = Common.FormatString(doc.DocumentNode.SelectSingleNode("//h3[@class='part-description']").InnerText);

            price.Supplier = "onlinecomponents";
            price.MoneyType = "USD";

            price.Prices = GetExactPriceTable();

            this.priceList.Add(price);
        }

        public List<Model.Price> GetExactPriceTable()
        {
            List<Model.Price> priceModels = new List<Model.Price>();
            string sectionPrices = "//table[@id='Pricetable']/tr";
            HtmlNodeCollection pricesNodes = doc.DocumentNode.SelectNodes(sectionPrices);
            int count = pricesNodes.Count();
            for (int i = 0; i < count; i++)
            {
                Model.Price priceModel = new Model.Price();
                HtmlNodeCollection priceNodes = pricesNodes[i].SelectNodes("td");
                string price = priceNodes[0].InnerText.Replace(":", "");
                if (i + 1 == count)
                {
                    priceModel.MaxQuantity = int.MaxValue;
                }
                else
                {
                    priceModel.MaxQuantity = int.Parse(Common.FormatString(price.Split('-')[1]));
                }

                priceModel.MinQuantity = int.Parse(Common.FormatString(price.Split('-')[0]));
                priceModel.UnitPrice = double.Parse(Common.FormatString(priceNodes[1].InnerText));
                priceModels.Add(priceModel);
            }

            return priceModels;
        }
        #endregion

        public string HandleStock(HtmlNode node)
        {
            string stock = "";
            if (node == null)
                return "";
            stock = Common.FormatString(node.InnerText);
            string pattern = @"\d{0,}";
            Regex reg = new Regex(pattern);
            Match result = reg.Match(stock);

            return result.Value;
        }
    }
}
