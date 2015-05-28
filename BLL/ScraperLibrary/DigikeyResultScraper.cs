using GrabbingParts.DAL.DataAccessCenter;
using System.Xml.Linq;
using GrabbingParts.Util.XmlHelpers;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlTypes;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrabbingParts.BLL.ScraperLibrary
{
    public class DigikeyResultScraper : ResultScraper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WXH");
        private const string SUPPLIER = "digikey";
        private const string MONEYTYPE = "¥";
        private string[] arrays = {"select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 100000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 200000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 300000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 400000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 500000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 600000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 700000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 800000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 900000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1000000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1100000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1200000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1300000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1400000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1500000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1600000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1700000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1800000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 1900000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2000000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2100000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2200000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2300000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2400000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2500000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2600000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2700000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2800000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 2900000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 3000000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 3100000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 3200000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 3300000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 3400000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN",
                                   "select top 100000 a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN and a.SupplierPN not in (select top 3500000 SupplierPN from [零件地址_digikey] order by SupplierPN) order by a.SupplierPN"
                                  };

        public override void Scrape()
        {
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();

            int scrapeResultIndex = Int32.Parse(ConfigurationManager.AppSettings["GetScrapeResultIndex"]);
            int endIndex = scrapeResultIndex + 5;

            for (int arrayIndex = scrapeResultIndex; arrayIndex <= endIndex; arrayIndex++)
            {
                int tmpIndex = arrayIndex;
                tasks.Add(Task.Factory.StartNew(() => ScrapeTask(tmpIndex)));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private void ScrapeTask(int arrayIndex)
        {
            log.InfoFormat("arrayIndex: {0}", arrayIndex.ToString());
            XElement partInfo = DataCenter.GetPartInfoFromDatabase(arrays[arrayIndex]);
            if(partInfo.HasElements)
            {                
                string partUrl;
                string keyPartNumberGuid;
                string KeyPartNumber;
                string supplierGuid = DataCenter.GetSupplierGuid(SUPPLIER);
                string partNumber;
                string manufacturerGuid;
                string manufacturer;
                string packing;
                string description;

                string quantityString;
                string tmpQuantity;
                int quantity;
                string quantityMinString;
                int quantityMin;
                string quantityMaxString;
                string tmpQuantityMax;
                int quantityMax;
                string unitPriceString; //e.g. ¥ 123.66000
                string tmpUnitPrice; //e.g. 123.66000
                decimal unitPrice; //e.g. 123.66000

                string stockParagraph;
                string stockPattern = "\\d+,?\\d*";
                string stockString;
                string tmpStock;
                int stock; //库存
                string priceXpath = "//table[@id='pricing']/tr[not(th)]";

                DataTable grabbingResultDataTable = new DataTable();
                InitDataTable(grabbingResultDataTable);

                foreach (XElement part in partInfo.Elements("p"))
                {
                    partUrl = XmlHelpers.GetAttribute(part, "url");
                    keyPartNumberGuid = XmlHelpers.GetAttribute(part, "keypnguid");
                    KeyPartNumber = XmlHelpers.GetAttribute(part, "pn");
                    partNumber = KeyPartNumber;
                    manufacturerGuid = XmlHelpers.GetAttribute(part, "manguid");
                    manufacturer = XmlHelpers.GetAttribute(part, "man");

                    HtmlDocument partHtmlDoc = Common.Common.RetryRequest(partUrl, partNumber);
                    if (partHtmlDoc != null)
                    {
                        packing = XmlHelpers.GetText(partHtmlDoc.DocumentNode, "//tr[th[contains(., '包装') and not(contains(., '标准包装'))]]/td");
                        description = XmlHelpers.GetText(partHtmlDoc.DocumentNode, "//table[@id='pricingTable']/tr[6]/td");

                        stockParagraph = XmlHelpers.GetText(partHtmlDoc.DocumentNode, "//table[@id='pricingTable']/tr[3]/td");
                        stockParagraph = Regex.Replace(stockParagraph, "<!DOCTYPE html.*transitional.dtd\">", "");
                        stockParagraph = Regex.Replace(stockParagraph, "<!-- WAS_NAME CHINA_PROD_SERVER\\d  -->", "");

                        Match match = Regex.Match(stockParagraph, stockPattern);
                        stockString = match.Value;
                        tmpStock = stockString.Replace("个", "").Replace(",", ""); //Todo: test this case
                        if (!Int32.TryParse(tmpStock, out stock))
                        {
                            stock = 0;
                        }

                        HtmlNodeCollection priceList = partHtmlDoc.DocumentNode.SelectNodes(priceXpath);
                        int priceCount = priceList.Count;
                        for (int i = 0; i < priceCount; i++)
                        {
                            HtmlNode price = priceList[i];
                            quantityString = XmlHelpers.GetText(price, "td[1]");
                            tmpQuantity = quantityString.Replace("个", "").Replace(",", ""); //Todo: test this case
                            if (!Int32.TryParse(tmpQuantity, out quantity))
                            {
                                quantity = 0;
                            }
                            quantityMinString = quantityString;
                            quantityMin = quantity;

                            HtmlNode nextPrice = (i == priceList.Count - 1) ? null : priceList[i + 1];
                            if (nextPrice != null)
                            {
                                quantityMaxString = XmlHelpers.GetText(nextPrice, "td[1]");
                                tmpQuantityMax = quantityMaxString.Replace("个", "").Replace(",", ""); //Todo: test this case
                                Int32.TryParse(tmpQuantityMax, out quantityMax);
                            }
                            else
                            {
                                quantityMax = Int32.MaxValue;
                            }

                            unitPriceString = XmlHelpers.GetText(price, "td[2]");
                            tmpUnitPrice = unitPriceString.Replace("¥", "").TrimStart(); //Todo: test this case
                            if (!Decimal.TryParse(tmpUnitPrice, out unitPrice))
                            {
                                unitPrice = 0.0m;
                            }

                            CheckFieldLength(ref quantityString, ref quantityMinString, ref unitPriceString,
                                             ref stockString, ref packing, ref description);

                            DataRow grabbingResult = grabbingResultDataTable.NewRow();
                            grabbingResult["GUID"] = (SqlGuid)System.Guid.NewGuid();
                            grabbingResult["CreateDate"] = DateTime.Now.Date;

                            grabbingResult["KeyPNGUID"] = keyPartNumberGuid;
                            grabbingResult["KeyPN"] = KeyPartNumber;
                            grabbingResult["SupplierGUID"] = supplierGuid;
                            grabbingResult["Supplier"] = SUPPLIER;
                            grabbingResult["PN"] = partNumber;
                            grabbingResult["ManufacturerGUID"] = manufacturerGuid;
                            grabbingResult["Manufacturer"] = manufacturer;
                            
                            grabbingResult["QtyStr"] = quantityString;
                            grabbingResult["Qty"] = quantity;
                            grabbingResult["QtyMinStr"] = quantityMinString;
                            grabbingResult["QtyMin"] = quantityMin;
                            grabbingResult["QtyMax"] = quantityMax;
                            grabbingResult["UnitPriceStr"] = unitPriceString;
                            grabbingResult["UnitPrice"] = unitPrice;
                            grabbingResult["MoneyType"] = MONEYTYPE;
                            grabbingResult["StockStr"] = stockString;
                            grabbingResult["Stock"] = stock;
                            grabbingResult["Packing"] = packing;
                            grabbingResult["Descript"] = description;

                            grabbingResultDataTable.Rows.Add(grabbingResult);
                        }

                        DataCenter.InsertDataToGrabbingResult(grabbingResultDataTable);
                    }
                }
            }
        }

        private void InitDataTable(DataTable grabbingResultDataTable)
        {
            grabbingResultDataTable.Columns.Add("GUID", typeof(System.Data.SqlTypes.SqlGuid));
            grabbingResultDataTable.Columns.Add("CreateDate", typeof(System.Data.SqlTypes.SqlDateTime));
            grabbingResultDataTable.Columns.Add("KeyPNGUID", typeof(string));
            grabbingResultDataTable.Columns.Add("KeyPN", typeof(string));
            grabbingResultDataTable.Columns.Add("SupplierGUID", typeof(string));
            grabbingResultDataTable.Columns.Add("Supplier", typeof(string));
            grabbingResultDataTable.Columns.Add("PN", typeof(string));
            grabbingResultDataTable.Columns.Add("ManufacturerGUID", typeof(string));
            grabbingResultDataTable.Columns.Add("Manufacturer", typeof(string));
            grabbingResultDataTable.Columns.Add("QtyStr", typeof(string));
            grabbingResultDataTable.Columns.Add("Qty", typeof(System.Data.SqlTypes.SqlInt32));
            grabbingResultDataTable.Columns.Add("QtyMinStr", typeof(string));
            grabbingResultDataTable.Columns.Add("QtyMin", typeof(System.Data.SqlTypes.SqlInt32));
            grabbingResultDataTable.Columns.Add("QtyMax", typeof(System.Data.SqlTypes.SqlInt32));
            grabbingResultDataTable.Columns.Add("UnitPriceStr", typeof(string));
            grabbingResultDataTable.Columns.Add("UnitPrice", typeof(System.Data.SqlTypes.SqlDecimal));
            grabbingResultDataTable.Columns.Add("MoneyType", typeof(string));
            grabbingResultDataTable.Columns.Add("StockStr", typeof(string));
            grabbingResultDataTable.Columns.Add("Stock", typeof(System.Data.SqlTypes.SqlInt32));
            grabbingResultDataTable.Columns.Add("Packing", typeof(string));
            grabbingResultDataTable.Columns.Add("Descript", typeof(string));
            grabbingResultDataTable.Columns.Add("Remark", typeof(string)); //Set it as NULL for digikey
        }

        private void CheckFieldLength(ref string quantityString, ref string quantityMinString, ref string unitPriceString,
            ref string stockString, ref string packing, ref string description)
        {
            if (quantityString.Length > 64)
            {
                quantityString = quantityString.Substring(0, 64);
            }

            if (quantityMinString.Length > 64)
            {
                quantityMinString = quantityMinString.Substring(0, 64);
            }

            if (unitPriceString.Length > 64)
            {
                unitPriceString = unitPriceString.Substring(0, 64);
            }

            if (stockString.Length > 64)
            {
                stockString = stockString.Substring(0, 64);
            }

            if (packing.Length > 64)
            {
                packing = packing.Substring(0, 64);
            }

            if (description.Length > 128)
            {
                description = description.Substring(0, 128);
            }
        }
    }
}
