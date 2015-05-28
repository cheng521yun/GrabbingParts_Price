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
    public class Alliedelec : BasePrice, IPrice
    {
        public static string url = "http://ex-en.alliedelec.com/search/results.aspx?term={0}";//M4T28-BR12SH1
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
            string baiscInf = "//table[@id='tblParts']/tr";
            HtmlNodeCollection infNodes = doc.DocumentNode.SelectNodes(baiscInf);

            if (infNodes == null)
                return;
            for (int i = 0; i < infNodes.Count()-1; i++)
            {
                if (i > 2)
                    GetSinglePart(infNodes[i]);
            }
        }

        public void GetSinglePart(HtmlNode singleNode)
        {            
            PriceResult price = new PriceResult();

            HtmlNode infNode = singleNode;
            HtmlNodeCollection infoNodes = infNode.SelectNodes("td");

            price.Manufacturer = Common.FormatString(infoNodes[2].SelectSingleNode("div/div[1]/a/b").InnerText);
            price.ManufacturerID = Common.FormatString(infoNodes[2].SelectSingleNode("div/div[2]/a").InnerText);

            if (infoNodes[4].SelectSingleNode("div") != null)
                return;

            price.Stock = Common.FormatString(infoNodes[4].SelectSingleNode("table/tr/td[2]").InnerText);


            price.Supplier = "alliedelec";
            price.MoneyType = "USD";

            price.Prices = GetPriceTable(infoNodes[5]);

            this.priceList.Add(price);
        }


        public List<Model.Price> GetPriceTable(HtmlNode priceNode)
        {
            if (priceNode != null)
            {
                List<Model.Price> priceModels = new List<Model.Price>();
                string sectionPrices = "table/tr[@nowrap='nowrap']";
                HtmlNodeCollection pricesNodes = priceNode.SelectNodes(sectionPrices);
                if (pricesNodes != null)
                {
                    int count = pricesNodes.Count();
                    for (int i = 0; i < count; i++)
                    {
                        Model.Price priceModel = new Model.Price();
                        HtmlNodeCollection currentNodes = pricesNodes[i].SelectNodes("td");

                        if (i + 1 == count)
                        {
                            priceModel.MaxQuantity = int.MaxValue;
                        }
                        else
                        {
                            HtmlNodeCollection nextNodes = pricesNodes[i + 1].SelectNodes("td");
                            priceModel.MaxQuantity = int.Parse(Common.FormatString(nextNodes[0].InnerText));
                        }

                        priceModel.MinQuantity = int.Parse(Common.FormatString(currentNodes[0].InnerText));
                        priceModel.UnitPrice = double.Parse(Common.FormatString(currentNodes[2].InnerText));

                        priceModels.Add(priceModel);
                    }
                }
                else
                {
                    Model.Price priceModel = new Model.Price();
                    priceModel.UnitPrice = double.Parse(Common.FormatString(priceNode.SelectSingleNode("b").InnerText));
                    priceModels.Add(priceModel);
                }
                return priceModels;
            }
            return null;
        }

        public string HandleStock(HtmlNode node)
        {
            string stock = "";
            if (node == null)
                return "";

            stock = Common.FormatString(node.InnerText);
            if (node.InnerText.IndexOf("暂无") > -1)
                return node.InnerText;
            else
            {
                string pattern = @"\d{0,}";
                Regex reg = new Regex(pattern);
                Match result = reg.Match(stock);

                return result.Value;
            }
        }
    }
}
