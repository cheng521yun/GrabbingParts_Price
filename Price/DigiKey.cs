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
    public class DigiKey : BasePrice, IPrice
    {
        public static string url = "http://www.digikey.com.cn/search/zh?x=0&y=0&lang=zh&site=cn&keywords={0}";//M4T28-BR12SH1
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

                this.priceList.Add(price);
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
            string baiscInf = "//table[@class='product-details-table']/tr/td/table[@id='pricingTable']/tr";
            HtmlNodeCollection infNodes = doc.DocumentNode.SelectNodes(baiscInf);
            if (infNodes == null)
            {
                price = null;
                return;
            }
            for (int i = 0; i < infNodes.Count(); i++)
            {
                HtmlNode node = infNodes.ElementAt(i);
                string nodeValue = node.SelectSingleNode("td").InnerText;
                nodeValue = Common.FormatString(nodeValue);
                switch (i)
                {
                    case 2: price.Stock = HandleStock(node,nodeValue); break;
                    case 3: price.Manufacturer = nodeValue; break;
                    case 4: price.ManufacturerID = nodeValue; break;
                    case 5: price.Description = nodeValue; break;
                }
            }

            GetPackage();

            GePricesTable();

            price.Supplier = "digikey";
            price.MoneyType = "CNY";

            if (string.IsNullOrEmpty(price.ManufacturerID))
                price = null;
        }

        /// <summary>
        /// Package
        /// </summary>
        public void GetPackage()
        {
            string generalInf = "//table[@id='GeneralInformationTable']/tr/td/table[@id='DatasheetsTable1']/tr/td";
            HtmlNodeCollection genNodes = doc.DocumentNode.SelectNodes(generalInf);
            if (genNodes != null && genNodes.ElementAt(1) != null)
            {
                price.Package = Common.FormatString(genNodes.ElementAt(1).InnerText);
            }
        }    

        /// <summary>
        /// 取分段价格 
        /// </summary>       
        public void GePricesTable()
        {
            List<Model.Price> priceModels = new List<Model.Price>();
            string sectionPrices = "//table[@class='product-details-table']/tr/td/table[@id='pricingTable']/tr/td/table[@id='pricing']/tr";
            HtmlNodeCollection priceNodes = doc.DocumentNode.SelectNodes(sectionPrices);
            List<string> prices = new List<string>();
            int nodesCount = priceNodes.Count();
            for (int i = 0; i < nodesCount; i++)
            {
                Model.Price priceModel = new Model.Price();
                HtmlNode currentNode = priceNodes.ElementAt(i);
                HtmlNodeCollection currentNodes = currentNode.SelectNodes("td");
                
                if (currentNodes != null && currentNodes.Count == 3)
                {
                    if (i + 1 != nodesCount)
                    {
                        HtmlNode nextNode = priceNodes.ElementAt(i + 1);
                        HtmlNodeCollection nextNodes = nextNode.SelectNodes("td");
                        priceModel.MaxQuantity = int.Parse(Common.FormatString(nextNodes[0].InnerText));
                    }
                    else
                        priceModel.MaxQuantity = int.MaxValue;                       

                    priceModel.MinQuantity = int.Parse(Common.FormatString(currentNodes[0].InnerText));
                    priceModel.UnitPrice = double.Parse(Common.FormatString(currentNodes[1].InnerText));

                    priceModels.Add(priceModel);
                }
            }

            price.Prices = priceModels;
        }

            

        /// <summary>
        /// e.g :BQ48SH-28X6NSH  
        /// </summary>
        /// <param name="node"></param>
        /// <param name="stock"></param>
        /// <returns></returns>
        public string HandleStock(HtmlNode node, string stock)
        {
            if (node.SelectSingleNode("td/input[@id='boQty']") != null)
                return "0";
            else
                return stock;
        }
    }
}

