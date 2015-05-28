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
    public class Microchipdirect : BasePrice, IPrice
    {
        public string url = "http://www.microchipdirect.com/ProductSearch.aspx?Keywords={0}";//ADM00641
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
            return priceList;
        }

        public void GetResult()
        {
            string baiscInf = "//div[@id='jtooltip']/div/table";
            HtmlNodeCollection infNodes = doc.DocumentNode.SelectNodes(baiscInf);

            if (infNodes == null)
                return;
            for (int i = 0; i < infNodes.Count(); i++)
            {
                GetSinglePart(infNodes[i]);
            }
        }
        
        public void GetSinglePart(HtmlNode singleNode)
        {            
            PriceResult price = new PriceResult();

            HtmlNode infNode = singleNode;
            HtmlNode PNNode = infNode.SelectSingleNode("tr[1]/td/table/tr[2]/td[2]/table/tr/td[1]");
            if (PNNode == null)
                return;
            price.ManufacturerID = Common.FormatString(Common.FormatString(PNNode.InnerText));
            price.Manufacturer = "";
            price.Description = "";

            HtmlNode StockNode = infNode.SelectSingleNode("tr[4]/td/table/tr[1]/td[3]/table/tr[1]/td/span");
            price.Stock = Common.FormatString(StockNode.InnerText.Split(':')[1]);
            
            price.Supplier = "microchipdirect";
            price.MoneyType = "CNY";

            GetPriceTable(infNode);
            price.Prices = GetPriceTable(infNode);

            priceList.Add(price);
        }


        public List<Model.Price> GetPriceTable(HtmlNode infNode)
        {
            HtmlNode priceNode1 = infNode.SelectSingleNode("tr[4]/td/table/tr[2]/td[1]");
            HtmlNode priceNode2 = infNode.SelectSingleNode("tr[4]/td/table/tr[2]/td[2]");
            List<Model.Price> priceModels = new List<Model.Price>();

            if (priceNode1 != null)
            {
                string sectionPrices1 = "div/table/tr[2]/td/table/tr";
                HtmlNodeCollection pricesNodes1 = priceNode1.SelectNodes(sectionPrices1);
                GetCommonPriceTable(priceModels, pricesNodes1);
            }

            if (priceNode2 != null)
            {
                string sectionPrices2 = "table/tr[1]/td/div/table/tr[2]/td/table/tr";
                HtmlNodeCollection pricesNodes2 = priceNode2.SelectNodes(sectionPrices2);
                GetVIPPriceTable(priceModels, pricesNodes2);
            }
            return priceModels;
        }

        public void GetCommonPriceTable(List<Model.Price> priceModels, HtmlNodeCollection pricesNodes)
        {

            int count = pricesNodes.Count();
            for (int i = 0; i < count; i++)
            {
                Model.Price priceModel = new Model.Price();
                HtmlNodeCollection currentNodes = pricesNodes[i].SelectNodes("td");

                if (i + 1 == count)
                {
                    priceModel.MaxQuantity = int.MaxValue;
                    priceModel.MinQuantity = int.Parse(Common.FormatString(currentNodes[0].InnerText));
                }
                else
                {
                    priceModel.MaxQuantity = int.Parse(Common.FormatString(currentNodes[0].InnerText.Split('-')[1]));
                    priceModel.MinQuantity = int.Parse(Common.FormatString(currentNodes[0].InnerText.Split('-')[0]));
                }

                
                priceModel.UnitPrice = double.Parse(Common.FormatString(currentNodes[2].InnerText));

                priceModels.Add(priceModel);
            }
        }
        public void GetVIPPriceTable(List<Model.Price> priceModels, HtmlNodeCollection pricesNodes)
        {

            int count = pricesNodes.Count();
            for (int i = 0; i < count-1; i++)
            {
                Model.Price priceModel = new Model.Price();
                HtmlNodeCollection currentNodes = pricesNodes[i].SelectNodes("td");

                if (i == count-2)
                {
                    priceModel.MaxQuantity = int.MaxValue;
                    priceModel.MinQuantity = int.Parse(Common.FormatString(currentNodes[0].InnerText));

                }
                else
                {
                    priceModel.MaxQuantity = int.Parse(Common.FormatString(currentNodes[0].InnerText.Split('-')[1]));
                    priceModel.MinQuantity = int.Parse(Common.FormatString(currentNodes[0].InnerText.Split('-')[0]));

                }

                priceModel.UnitPrice = double.Parse(Common.FormatString(currentNodes[2].InnerText));

                priceModels.Add(priceModel);
            }
        }
    }
}
