using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using GrabbingParts.BLL.Types;
using GrabbingParts.DAL.DataAccessCenter;
using GrabbingParts.Util.StringHelpers;
using GrabbingParts.Util.XmlHelpers;
using HtmlAgilityPack;
using System.Configuration;
using System.Text.RegularExpressions;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace GrabbingParts.BLL.ScraperLibrary
{
    public class DigikeyScraper : Scraper
    {
        private const string SUPPLIERNAME = "digikey";
        private const string DIGIKEYHOMEURL = "http://www.digikey.com.cn";
        private const string BAOZHUANG = "包装";
        private const string XIANGGUANCHANPIN = "相关产品";        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WXH");
        private Dictionary<string, SqlGuid> manufacturerDictionary = new Dictionary<string, SqlGuid>();
        private Object obj = new Object();
        private Object obj1 = new Object();
        private List<string> supplierPartNumbers = new List<string>();
        private string[] arrays = {"select top 100000 SupplierPN from [产品资料]",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 100000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 200000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 300000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 400000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 500000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 600000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 700000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 800000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 900000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1000000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1100000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1200000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1300000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1400000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1500000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1600000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1700000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1800000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 1900000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2000000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2100000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2200000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2300000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2400000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2500000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2600000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2700000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2800000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 100000 SupplierPN from [产品资料] where SupplierPN not in (select top 2900000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN",
                                   "select top 30000 SupplierPN from [产品资料] where SupplierPN not in (select top 3000000 SupplierPN from [产品资料] order by SupplierPN) order by SupplierPN"
                                  };
        public bool IsGetSupplierPartNumberFromDatabase
        {
            get { return ConfigurationManager.AppSettings["GetSupplierPartNumberFromDatabase"] == "true"; }
        }

        public override void ScrapePage()
        {
            log.InfoFormat( "run ScrapePage()" );

            Stopwatch sw = Stopwatch.StartNew();
            
            sw = Stopwatch.StartNew();

            GetSupplierPartNumberFromDatabase();

            GetCategory();

            if (ConfigurationManager.AppSettings["InsertDataToSupplier"] == "true")
            {
                InsertDataToSupplier();
            }         

            sw.Stop();
            log.InfoFormat("Scraping finish.cost:{0}ms", sw.ElapsedMilliseconds);
        }

        private void GetSupplierPartNumberFromDatabase()
        {
            if (IsGetSupplierPartNumberFromDatabase)
            {
                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();

                for (int arrayIndex = 0; arrayIndex <= 30; arrayIndex+=3)
                {
                    int tmpIndex = arrayIndex;
                    tasks.Add(Task.Factory.StartNew(() => GetSupplierPartNumberTask(tmpIndex)));
                }
                Task.WaitAll(tasks.ToArray());
            }
        }

        private void GetSupplierPartNumberTask(int arrayIndex)
        {
            AddSupplierPartNumber(DataCenter.GetSupplierPartNumberFromDatabase(arrays[arrayIndex]));
            if (arrayIndex != 30)
            {
                AddSupplierPartNumber(DataCenter.GetSupplierPartNumberFromDatabase(arrays[arrayIndex + 1]));
                AddSupplierPartNumber(DataCenter.GetSupplierPartNumberFromDatabase(arrays[arrayIndex] + 2));
            }            
        }

        private void AddSupplierPartNumber(XElement partInfo)
        {
            if (partInfo != null && partInfo.HasElements)
            {
                string supplierPartNumber;
                foreach (XElement part in partInfo.Elements("p"))
                {
                    lock(obj1)
                    {
                        supplierPartNumber = XmlHelpers.GetAttribute(part, "suppn");
                        if (!supplierPartNumbers.Contains(supplierPartNumber))
                        {
                            supplierPartNumbers.Add(supplierPartNumber);
                        }
                    }                    
                }
            }
        }

        /// <summary>
        /// Get part group, use task in Category level
        /// </summary>
        private void GetCategory()
        {
            string category = ConfigurationManager.AppSettings["Category"];

            log.InfoFormat( "GitHub Commit 25;  {0}", category );

            if (category == "all")
            {
                HandleCategory("半导体产品", "Category1");
                HandleCategory("无源元件", "Category2");
                HandleCategory("互连产品", "Category3");
                HandleCategory("机电产品", "Category4");
                HandleCategory("光电元件", "Category5");
            }
            else
            {
                switch(category)
                {
                    case "Category1":
                        HandleCategory("半导体产品", "Category1");
                        break;
                    case "Category2":
                        HandleCategory("无源元件", "Category2");
                        break;
                    case "Category3":
                        HandleCategory("互连产品", "Category3");
                        break;
                    case "Category4":
                        HandleCategory("机电产品", "Category4");
                        break;
                    case "Category5":
                        HandleCategory("光电元件", "Category5");
                        break;
                }
            }
        }

        private void HandleCategory(string categoryName, string categoryIndex)
        {
            Category category = new Category(categoryIndex, categoryName);
            XElement scrapedData;
         
            if(!(ConfigurationManager.AppSettings["GetCategoryFromXmlFile"] == "true"))
            {
                Stopwatch sw = Stopwatch.StartNew();
                GetWidget(category, categoryIndex);

                sw.Stop();
                log.InfoFormat("GetWidget for category {0} finish.cost:{1}ms", categoryName, sw.ElapsedMilliseconds);

                sw = Stopwatch.StartNew();
                scrapedData = PrepareScrapedData(category);

                sw.Stop();
                log.InfoFormat("PrepareScrapedData for category {0} finish.cost:{0}ms", categoryIndex, sw.ElapsedMilliseconds);

                SaveScrapedData(scrapedData, categoryIndex);
            }
            else
            {
                scrapedData = XElement.Load(string.Format("{0}.xml", categoryIndex));
            }

            if (ConfigurationManager.AppSettings["InsertCategoryToDatabase"] == "true")
            {
                List<DataTable> categoryDataTables = new List<DataTable>();

                InitCategoryDataTables(categoryDataTables);

                InsertCategoryToDatabase(scrapedData, categoryDataTables);
            }

            InsertPrimaryDataToDatabase(scrapedData);

            log.InfoFormat("Task for category {0} finished.", categoryName);
        }

        private void InsertCategoryToDatabase(XElement scrapedData, List<DataTable> categoryDataTables)
        {
            SqlGuid guid0;
            SqlGuid guid1;
            SqlGuid guid2;
            XElement category = scrapedData.Element("cat");

            guid0 = new SqlGuid(XmlHelpers.GetAttribute(category, "guid"));
            AddRow(categoryDataTables[0], category, guid0, "");

            foreach (XElement widget in category.XPathSelectElements("wgts/wgt"))
            {
                guid1 = new SqlGuid(XmlHelpers.GetAttribute(widget, "guid"));
                AddRow(categoryDataTables[1], widget, guid1, guid0.ToString());

                foreach (XElement partGroup in widget.XPathSelectElements("pgs/pg"))
                {
                    guid2 = new SqlGuid(XmlHelpers.GetAttribute(partGroup, "guid"));
                    AddRow(categoryDataTables[2], partGroup, guid2, guid1.ToString());
                }
            }

            DataCenter.InsertDataToCategory(categoryDataTables);
        }

        /// <summary>
        /// Get sub category and widget
        /// </summary>
        private void GetWidget(Category category, string categoryIndex)
        {
            string categoryUrl = ConfigurationManager.AppSettings[categoryIndex];
            HtmlDocument categoryHtmlDoc = Common.Common.RetryRequest(categoryUrl);
            if(categoryHtmlDoc != null)
            {
                string xpathForWidget = "//span[@class='catfiltertopitem']";
                string xpathForWidgetName = ".//h2/a";
                string xpathForPartGroup = "ul/li";
                HtmlNodeCollection liAnchorList = categoryHtmlDoc.DocumentNode.SelectNodes(xpathForWidget);
                if(liAnchorList != null)
                {
                    string widgetName;
                    string widgetUrl;
                    int widgetId = 1;
                    string partGroupName;
                    string partGroupUrl;
                    Widget widget;
                    foreach(HtmlNode liAnchor in liAnchorList)
                    {
                        widgetName = HttpUtility.HtmlDecode(XmlHelpers.GetText(liAnchor.SelectSingleNode(xpathForWidgetName)));
                        widgetName = widgetName.TrimStart().TrimEnd();
                        widgetUrl = DIGIKEYHOMEURL + XmlHelpers.GetAttribute(liAnchor.SelectSingleNode(xpathForWidgetName), "href");

                        widget = new Widget(widgetId.ToString(), widgetName, widgetUrl);
                        category.Widgets.Add(widget);
                        widgetId++;

                        HtmlNodeCollection partGroupList = liAnchor.SelectNodes(xpathForPartGroup);
                        int partGroupId = 1;
                        foreach(HtmlNode partGroup in partGroupList)
                        {                            
                            partGroupName = HttpUtility.HtmlDecode(XmlHelpers.GetText(partGroup.SelectSingleNode("a")));
                            partGroupName = Regex.Replace(partGroupName, @" \([^\(]*\)", "");
                            partGroupUrl = DIGIKEYHOMEURL + XmlHelpers.GetAttribute(partGroup.SelectSingleNode("a"), "href");
                            widget.PartGroups.Add(new PartGroup(partGroupId.ToString(), partGroupName, partGroupUrl));
                            partGroupId++;
                        }
                    }
                }
            }
        }

        private void SaveScrapedData(XElement scrapedData, string categoryIndex)
        {
            try
            {
                string fileName = string.Format("{0}.xml", categoryIndex);
                scrapedData.Save(@"c:\" + fileName);
            }
            catch(Exception ex)
            {
                log.Error("Error in SaveScrapedData", ex);
            }
        }

        private void InsertDataToSupplier()
        {
            Stopwatch sw = Stopwatch.StartNew();

            DataTable supplierDataTable = new DataTable();
            supplierDataTable.Columns.Add("GUID", typeof(System.Data.SqlTypes.SqlGuid));
            supplierDataTable.Columns.Add("Supplier", typeof(string));
            supplierDataTable.Columns.Add("WebUrl", typeof(string));

            DataRow drSupplier = supplierDataTable.NewRow();
            drSupplier["GUID"] = (SqlGuid)System.Guid.NewGuid();
            drSupplier["Supplier"] = SUPPLIERNAME;
            drSupplier["WebUrl"] = DIGIKEYHOMEURL;
            supplierDataTable.Rows.Add(drSupplier);

            DataCenter.InsertDataToSupplier(supplierDataTable);

            sw.Stop();
            log.InfoFormat("Task InsertDataToSupplier finished, cost{0}ms.", sw.ElapsedMilliseconds);
        }

        private void CheckFieldLength(ref string partId, ref string manufacturer, ref string description,
                                      ref string packing, ref string standardPacking, ref string datasheetsUrl,
                                      ref string imageUrl, ref string zoomImageUrl)
        {
            if (partId.Length > 64)
            {
                partId = partId.Substring(0, 64);
            }

            if (manufacturer.Length > 64)
            {
                manufacturer = manufacturer.Substring(0, 64);
            }

            if (description.Length > 64)
            {
                description = description.Substring(0, 64);
            }

            if (packing.Length > 64)
            {
                packing = packing.Substring(0, 64);
            }

            if (standardPacking.Length > 10)
            {
                standardPacking = standardPacking.Substring(0, 10);
            }

            if (datasheetsUrl.Length > 128)
            {
                datasheetsUrl = datasheetsUrl.Substring(0, 128);
            }

            if (imageUrl.Length > 128)
            {
                imageUrl = imageUrl.Substring(0, 128);
            }

            if (zoomImageUrl.Length > 128)
            {
                zoomImageUrl = zoomImageUrl.Substring(0, 128);
            }
        }

        /// <summary>
        /// Add pagesize to parts url
        /// </summary>
        /// <param name="partsUrl"></param>
        /// <returns></returns>
        private string AddPageNumberToPartsUrl(string partsUrl, string pageNumber)
        {
            string symbol = (partsUrl.IndexOf("?") > 0) ? "&" : "?";
            return partsUrl + symbol + "pageNumber=" + pageNumber;
        }

        /// <summary>
        /// Prepare scraped data
        /// </summary>
        private XElement PrepareScrapedData(Category category)
        {
            XElement result = new XElement("r");
            XElement xeCategory = new XElement("cat", new XAttribute("id", category.Id),
                                                                    new XAttribute("n", category.Name.TrimStart()),
                                                                    new XAttribute("guid", System.Guid.NewGuid().ToString()),
                                                                    new XElement("wgts"));
            result.Add(xeCategory);

            foreach (Widget widget in category.Widgets)
            {
                XElement xeWidget = new XElement("wgt", new XAttribute("id", widget.Id),
                                                        new XAttribute("n", widget.Name),
                                                        new XAttribute("url", widget.Url),
                                                        new XAttribute("guid", System.Guid.NewGuid().ToString()),
                                                        new XElement("pgs"));
                xeCategory.Element("wgts").Add(xeWidget);

                foreach (PartGroup partGroup in widget.PartGroups)
                {
                    XElement xePartGroup = new XElement("pg", new XAttribute("id", partGroup.Id),
                                                                new XAttribute("n", partGroup.Name),
                                                                new XAttribute("url", partGroup.Url),
                                                                new XAttribute("guid", System.Guid.NewGuid().ToString()),
                                                                new XElement("ps"));
                    xeWidget.Element("pgs").Add(xePartGroup);
                }
            }

            return result;
        }

        /// <summary>
        /// Init category data tables
        /// </summary>
        private void InitCategoryDataTables(List<DataTable> categoryDataTables)
        {
            for (int i = 0; i < 3; i++)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("GUID", typeof(System.Data.SqlTypes.SqlGuid));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("ParentID", typeof(string));
                dt.Columns.Add("Comment", typeof(string));
                categoryDataTables.Add(dt);
            }
        }

        /// <summary>
        /// Init primary data tables
        /// </summary>
        private void InitDataTables(DataTable productSpecDataTable, DataTable manufacturerDataTable,
            DataTable productInfoDataTable, DataTable partAddressDataTable)
        {
            productSpecDataTable.Columns.Add("GUID", typeof(System.Data.SqlTypes.SqlGuid));
            productSpecDataTable.Columns.Add("PN", typeof(string));
            productSpecDataTable.Columns.Add("Name", typeof(string));
            productSpecDataTable.Columns.Add("Content", typeof(string));

            manufacturerDataTable.Columns.Add("GUID", typeof(System.Data.SqlTypes.SqlGuid));
            manufacturerDataTable.Columns.Add("Manufacturer", typeof(string));

            productInfoDataTable.Columns.Add("GUID", typeof(System.Data.SqlTypes.SqlGuid));
            productInfoDataTable.Columns.Add("PN", typeof(string));
            productInfoDataTable.Columns.Add("SupplierPN", typeof(string));
            productInfoDataTable.Columns.Add("Manufacturer", typeof(string));
            productInfoDataTable.Columns.Add("ManufacturerID", typeof(System.Data.SqlTypes.SqlGuid));
            productInfoDataTable.Columns.Add("Description", typeof(string));
            productInfoDataTable.Columns.Add("Packing", typeof(string));
            productInfoDataTable.Columns.Add("StandardPacking", typeof(string));
            productInfoDataTable.Columns.Add("Type1", typeof(string));
            productInfoDataTable.Columns.Add("Type2", typeof(string));
            productInfoDataTable.Columns.Add("Type3", typeof(string));
            productInfoDataTable.Columns.Add("DatasheetsUrl", typeof(string));
            productInfoDataTable.Columns.Add("ImageUrl", typeof(string));
            productInfoDataTable.Columns.Add("ZoomImageUrl", typeof(string));

            partAddressDataTable.Columns.Add("GUID", typeof(System.Data.SqlTypes.SqlGuid));
            partAddressDataTable.Columns.Add("ManufacturerPN", typeof(string));
            partAddressDataTable.Columns.Add("SupplierPN", typeof(string));
            partAddressDataTable.Columns.Add("PartUrl", typeof(string));
        }

        private void InsertPrimaryDataToDatabase(XElement scrapedData)
        {            
            SqlGuid guid1;
            string partsUrl;
            XElement category = scrapedData.Element("cat");
            SqlGuid guid0 = new SqlGuid(XmlHelpers.GetAttribute(category, "guid"));
            bool insertSpecialWidgetToDatabase = ConfigurationManager.AppSettings["InsertSpecialWidgetToDatabase"] == "true";
            bool deleteSpecialWidgetFromDatabase = ConfigurationManager.AppSettings["DeleteSpecialWidgetFromDatabase"] == "true";
            string xpathForWidget = insertSpecialWidgetToDatabase ? "wgts/wgt[@get='1']" : "wgts/wgt";
            string widgetGuid;

            foreach (XElement widget in category.XPathSelectElements(xpathForWidget))
            {
                widgetGuid = XmlHelpers.GetAttribute(widget, "guid");
                if (deleteSpecialWidgetFromDatabase)
                {
                    DataCenter.DeleteSpecialWidgetFromDatabase(widgetGuid);
                }

                guid1 = new SqlGuid(widgetGuid);
                foreach (XElement partGroup in widget.XPathSelectElements("pgs/pg"))
                {
                    partsUrl = XmlHelpers.GetAttribute(partGroup, "url");
                    AddParts(partGroup, guid0, guid1, partsUrl);
                }
            }
        }

        private void AddParts(XElement partGroup, SqlGuid guid0, SqlGuid guid1, string partsUrl)
        {
            string currentPageXpath = "//div[@id='partSearchResults']/table/tr[1]/td[@align='left']";
            string partXpath = "//table[@id='productTable']//thead/tr[@class='tblevenrow' or @class='tbloddrow']";
            SqlGuid guid2 = new SqlGuid(XmlHelpers.GetAttribute(partGroup, "guid"));

            HtmlDocument partsHtmlDoc = Common.Common.RetryRequest(partsUrl);
            if (partsHtmlDoc != null)
            {
                HtmlNodeCollection trList = partsHtmlDoc.DocumentNode.SelectNodes(partXpath);
                if (trList != null)
                {
                    AddParts3(partGroup, guid0, guid1, guid2, trList);

                    HtmlNode currentPageNode = partsHtmlDoc.DocumentNode.SelectSingleNode(currentPageXpath);
                    string currentPageValue = XmlHelpers.GetText(currentPageNode);
                    if(currentPageValue != "")
                    {
                        string pattern = "\\d{1,4}/\\d{1,4}";
                        Match match = Regex.Match(currentPageValue, pattern);
                        currentPageValue = match.Value;

                        string tmpTotalPage = StringHelpers.GetLastDirectory(currentPageValue);
                        int totalPage = 0;
                        Int32.TryParse(tmpTotalPage, out totalPage);
                        int currentPage = 1;

                        if (totalPage > currentPage)
                        {
                            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                            int taskLimitCount = 10;
                            if (totalPage - 1 <= taskLimitCount)
                            {
                                for (int nextPage = 2; nextPage <= totalPage; nextPage++)
                                {
                                    string nextPageUrl = AddPageNumberToPartsUrl(partsUrl, nextPage.ToString());
                                    tasks.Add(System.Threading.Tasks.Task.Factory.StartNew(() => AddParts2(partGroup, guid0, guid1, nextPageUrl)));
                                }
                            }
                            else
                            {
                                int pageCountByTask = (int)Math.Ceiling((totalPage - 1) / (double)taskLimitCount);
                                for (int i = 0; i < taskLimitCount; i++)
                                {
                                    int taskIndex = i;
                                    tasks.Add(System.Threading.Tasks.Task.Factory.StartNew(() => AddPartsTask(taskIndex, pageCountByTask, totalPage, partGroup, guid0, guid1, partsUrl)));
                                }
                            }

                            Task.WaitAll(tasks.ToArray());
                        }
                    }
                }
                else
                {
                    HandleOnePart(partGroup, guid0, guid1, guid2, partsUrl, partsHtmlDoc);
                }
            }
            else
            {
                log.InfoFormat("Error in AddParts, partsHtmlDoc is null, partsUrl is: {0}", partsUrl);
            }
        }

        private void AddPartsTask(int taskIndex, int pageCountByTask, int totalPage, XElement partGroup, SqlGuid guid0, SqlGuid guid1, string currentPartsUrl)
        {
            //totalPage=256
            //taskIndex=0: nextPage = 2, 3, ... 27
            //taskIndex=1: nextPage = 28, 29, ... 53
            //taskIndex=2: nextPage = 54...
            //...
            //taskIndex=9: 
            int nextPage = 0;
            for (int i = 1; i <= pageCountByTask; i++)
            {
                nextPage = taskIndex * pageCountByTask + 1 + i;
                if(nextPage > totalPage)
                {
                    break;
                }
                else
                {
                    string nextPageUrl = AddPageNumberToPartsUrl(currentPartsUrl, nextPage.ToString());
                    AddParts2(partGroup, guid0, guid1, nextPageUrl);
                }                
            }            
        }

        private void AddParts2(XElement partGroup, SqlGuid guid0, SqlGuid guid1, string partsUrl)
        {
            string partXpath = "//table[@id='productTable']//thead/tr[@class='tblevenrow' or @class='tbloddrow']";
            SqlGuid guid2 = new SqlGuid(XmlHelpers.GetAttribute(partGroup, "guid"));

            HtmlDocument partsHtmlDoc = Common.Common.RetryRequest(partsUrl);
            if (partsHtmlDoc != null)
            {
                HtmlNodeCollection trList = partsHtmlDoc.DocumentNode.SelectNodes(partXpath);
                if (trList != null)
                {
                    AddParts3(partGroup, guid0, guid1, guid2, trList);
                }
                else
                {
                    //Todo: add the partGroupUrl to log file
                }
            }
        }

        private void AddParts3(XElement partGroup, SqlGuid guid0, SqlGuid guid1, SqlGuid guid2, HtmlNodeCollection trList)
        {
            string partDetailXpath = "//tr[th[contains(., '包装') and not(contains(., '标准包装'))]]/td";
            string xpathForStandardPacking = "//tr[th[contains(., '标准包装')]]/td";
            string productSpecXpath = "//table[@id='SpecificationTable1']/tr";
            string partId;
            string supplierPartId;
            string partUrl;
            string manufacturer;
            string description;
            string zoomImageUrl;
            string imageUrl;
            string datasheetUrl;
            string packing;
            string standardPacking;
            SqlGuid manufacturerGuid;

            DataTable productSpecDataTable = new DataTable();
            DataTable manufacturerDataTable = new DataTable();
            DataTable productInfoDataTable = new DataTable();
            DataTable partAddressDataTable = new DataTable();

            InitDataTables(productSpecDataTable, manufacturerDataTable, productInfoDataTable, partAddressDataTable);

            foreach (HtmlNode tr in trList)
            {
                supplierPartId = XmlHelpers.GetText(tr, "td/a[@id='digikeyPartNumberLnk']");
                if (NeedInsertDataToDatabase(supplierPartId))
                {
                    partId = XmlHelpers.GetText(tr, "td/a[@id='manufacturerPartNumberLnk']");
                    partUrl = DIGIKEYHOMEURL + XmlHelpers.GetAttribute(tr.SelectSingleNode("td/a[@id='manufacturerPartNumberLnk']"), "href");
                    manufacturer = XmlHelpers.GetText(tr, "td[12]");
                    description = XmlHelpers.GetText(tr, "td[10]");
                    zoomImageUrl = XmlHelpers.GetAttribute(tr.SelectSingleNode("//img[@class='pszoomer']"), "src");
                    imageUrl = XmlHelpers.GetAttribute(tr.SelectSingleNode("//img[@class='pszoomer']"), "zoomimg");
                    datasheetUrl = XmlHelpers.GetAttribute(tr.SelectSingleNode("td[2]/center/a"), "href");

                    HtmlDocument partHtmlDoc = Common.Common.RetryRequest(partUrl, partId);
                    HtmlNodeCollection productSpecList = null;

                    if (partHtmlDoc != null)
                    {
                        packing = XmlHelpers.GetText(partHtmlDoc.DocumentNode, partDetailXpath);
                        standardPacking = XmlHelpers.GetText(partHtmlDoc.DocumentNode, xpathForStandardPacking);
                        productSpecList = partHtmlDoc.DocumentNode.SelectNodes(productSpecXpath);
                    }
                    else
                    {
                        packing = "";
                        standardPacking = "";
                    }

                    if (partId != "" && manufacturer != "")
                    {
                        CheckFieldLength(ref partId, ref manufacturer, ref description, ref packing, ref standardPacking, ref datasheetUrl,
                            ref imageUrl, ref zoomImageUrl);
                        DataRow drProductInfo = productInfoDataTable.NewRow();
                        drProductInfo["GUID"] = (SqlGuid)System.Guid.NewGuid();
                        drProductInfo["PN"] = partId;
                        drProductInfo["SupplierPN"] = supplierPartId;
                        drProductInfo["Manufacturer"] = manufacturer;

                        lock (obj)
                        {
                            if (!manufacturerDictionary.ContainsKey(manufacturer))
                            {
                                manufacturerGuid = (SqlGuid)System.Guid.NewGuid();
                                manufacturerDictionary.Add(manufacturer, manufacturerGuid);

                                DataRow drManufacturer = manufacturerDataTable.NewRow();
                                drManufacturer["GUID"] = manufacturerGuid;
                                drManufacturer["Manufacturer"] = manufacturer;
                                manufacturerDataTable.Rows.Add(drManufacturer);

                                drProductInfo["ManufacturerID"] = manufacturerGuid;
                            }
                            else
                            {
                                drProductInfo["ManufacturerID"] = manufacturerDictionary[manufacturer];
                            }
                        }

                        drProductInfo["Description"] = description;
                        drProductInfo["Packing"] = packing;
                        drProductInfo["StandardPacking"] = standardPacking;
                        drProductInfo["Type1"] = guid0.ToString().ToUpper();
                        drProductInfo["Type2"] = guid1.ToString().ToUpper();
                        drProductInfo["Type3"] = guid2.ToString().ToUpper();
                        drProductInfo["DatasheetsUrl"] = datasheetUrl;
                        drProductInfo["ImageUrl"] = imageUrl;
                        drProductInfo["ZoomImageUrl"] = zoomImageUrl;
                        productInfoDataTable.Rows.Add(drProductInfo);

                        PrepareProductSpecDataTable(productSpecDataTable, productSpecList, partId);

                        DataRow drPartAddress = partAddressDataTable.NewRow();
                        drPartAddress["GUID"] = (SqlGuid)System.Guid.NewGuid();
                        drPartAddress["ManufacturerPN"] = partId;
                        drPartAddress["SupplierPN"] = supplierPartId;
                        drPartAddress["PartUrl"] = partUrl;
                        partAddressDataTable.Rows.Add(drPartAddress);
                    }
                    else
                    {
                        log.InfoFormat("PartId: {0}, Manufacturer: {1}", partId, manufacturer);
                    }
                }
            }

            DataCenter.ExecuteTransaction(productSpecDataTable, manufacturerDataTable, productInfoDataTable, partAddressDataTable);
        }

        private bool NeedInsertDataToDatabase(string supplierPartNumber)
        {
            if (IsGetSupplierPartNumberFromDatabase)
            {
                return !supplierPartNumbers.Contains(supplierPartNumber);
            }
            else
            {
                return true;
            }
        }

        private void HandleOnePart(XElement partGroup, SqlGuid guid0, SqlGuid guid1, SqlGuid guid2, string partsUrl, HtmlDocument partHtmlDoc)
        {
            bool onlyHasOnePart = XmlHelpers.GetAttribute(partGroup, "onepart") == "1";
            if (onlyHasOnePart)
            {
                string partDetailXpath = "//tr[th[contains(., '包装') and not(contains(., '标准包装'))]]/td";
                string xpathForStandardPacking = "//tr[th[contains(., '标准包装')]]/td";
                string productSpecXpath = "//table[@id='SpecificationTable1']/tr";

                DataTable productSpecDataTable = new DataTable();
                DataTable manufacturerDataTable = new DataTable();
                DataTable productInfoDataTable = new DataTable();
                DataTable partAddressDataTable = new DataTable();

                InitDataTables(productSpecDataTable, manufacturerDataTable, productInfoDataTable, partAddressDataTable);

                string partId = XmlHelpers.GetText(partHtmlDoc.DocumentNode.SelectSingleNode("//table[@id='pricingTable']/tr[5]/td/h1[@class='seohtag']"));
                string supplierPartId = XmlHelpers.GetText(partHtmlDoc.DocumentNode.SelectSingleNode("//table[@id='pricingTable']/tr[2]/td[@id='PartNumber']"));
                string partUrl = partsUrl;
                string manufacturer = XmlHelpers.GetText(partHtmlDoc.DocumentNode.SelectSingleNode("//table[@id='pricingTable']/tr[4]/td/h2[@class='seohtag']"));
                string description = XmlHelpers.GetText(partHtmlDoc.DocumentNode.SelectSingleNode("//table[@id='pricingTable']/tr[6]/td"));
                string zoomImageUrl = "";
                string imageUrl = "";
                string datasheetUrl = "";                
                string packing = XmlHelpers.GetText(partHtmlDoc.DocumentNode, partDetailXpath);
                string standardPacking = XmlHelpers.GetText(partHtmlDoc.DocumentNode, xpathForStandardPacking);
                HtmlNodeCollection productSpecList = partHtmlDoc.DocumentNode.SelectNodes(productSpecXpath);

                if (partId != "" && manufacturer != "")
                {
                    CheckFieldLength(ref partId, ref manufacturer, ref description, ref packing, ref standardPacking, ref datasheetUrl,
                        ref imageUrl, ref zoomImageUrl);
                    DataRow drProductInfo = productInfoDataTable.NewRow();
                    drProductInfo["GUID"] = (SqlGuid)System.Guid.NewGuid();
                    drProductInfo["PN"] = partId;
                    drProductInfo["SupplierPN"] = supplierPartId;
                    drProductInfo["Manufacturer"] = manufacturer;

                    lock (obj)
                    {
                        if (!manufacturerDictionary.ContainsKey(manufacturer))
                        {
                            SqlGuid manufacturerGuid = (SqlGuid)System.Guid.NewGuid();
                            manufacturerDictionary.Add(manufacturer, manufacturerGuid);

                            DataRow drManufacturer = manufacturerDataTable.NewRow();
                            drManufacturer["GUID"] = manufacturerGuid;
                            drManufacturer["Manufacturer"] = manufacturer;
                            manufacturerDataTable.Rows.Add(drManufacturer);

                            drProductInfo["ManufacturerID"] = manufacturerGuid;
                        }
                        else
                        {
                            drProductInfo["ManufacturerID"] = manufacturerDictionary[manufacturer];
                        }
                    }

                    drProductInfo["Description"] = description;
                    drProductInfo["Packing"] = packing;
                    drProductInfo["StandardPacking"] = standardPacking;
                    drProductInfo["Type1"] = guid0.ToString().ToUpper();
                    drProductInfo["Type2"] = guid1.ToString().ToUpper();
                    drProductInfo["Type3"] = guid2.ToString().ToUpper();
                    drProductInfo["DatasheetsUrl"] = datasheetUrl;
                    drProductInfo["ImageUrl"] = imageUrl;
                    drProductInfo["ZoomImageUrl"] = zoomImageUrl;
                    productInfoDataTable.Rows.Add(drProductInfo);

                    PrepareProductSpecDataTable(productSpecDataTable, productSpecList, partId);

                    DataRow drPartAddress = partAddressDataTable.NewRow();
                    drPartAddress["GUID"] = (SqlGuid)System.Guid.NewGuid();
                    drPartAddress["ManufacturerPN"] = partId;
                    drPartAddress["SupplierPN"] = supplierPartId;
                    drPartAddress["PartUrl"] = partUrl;
                    partAddressDataTable.Rows.Add(drPartAddress);
                }
                else
                {
                    log.InfoFormat("PartId: {0}, Manufacturer: {1}", partId, manufacturer);
                }

                DataCenter.ExecuteTransaction(productSpecDataTable, manufacturerDataTable, productInfoDataTable, partAddressDataTable);
            }
            else
            {
                log.InfoFormat("Error in HandleOnePart, trList is null, partsUrl is: {0}", partsUrl);
            }            
        }

        private void PrepareProductSpecDataTable(DataTable productSpecDataTable, HtmlNodeCollection productSpecList, string partId)
        {
            string productSpecName;
            string productSpecContent;
            int partIdLength = 64;
            int productSpecContentLength = 64;
            int productSpecNameLength = 32;

            if (productSpecList != null)
            {
                foreach (HtmlNode node in productSpecList)
                {
                    productSpecName = XmlHelpers.GetText(node, "th");
                    productSpecName = HttpUtility.HtmlDecode(productSpecName);

                    productSpecContent = XmlHelpers.GetText(node, "td");
                    productSpecContent = HttpUtility.HtmlDecode(productSpecContent);

                    if (!productSpecName.Contains(BAOZHUANG) && !productSpecName.Contains(XIANGGUANCHANPIN))
                    {
                        if (partId.Length > partIdLength)
                        {
                            partId = partId.Substring(0, partIdLength);
                        }

                        if (productSpecName.Length > productSpecNameLength)
                        {
                            productSpecName = productSpecName.Substring(0, productSpecNameLength);
                        }

                        if (productSpecContent.Length > productSpecContentLength)
                        {
                            productSpecContent = productSpecContent.Substring(0, productSpecContentLength);
                        }

                        DataRow dr = productSpecDataTable.NewRow();
                        dr["GUID"] = (SqlGuid)System.Guid.NewGuid();
                        dr["PN"] = partId;
                        dr["Name"] = productSpecName;
                        dr["Content"] = productSpecContent;
                        productSpecDataTable.Rows.Add(dr);
                    }
                }
            }
        }

        private void AddRow(DataTable dt, XElement type, SqlGuid currentGuid, string parentId)
        {
            DataRow dr = dt.NewRow();
            dr["GUID"] = currentGuid;
            dr["Name"] = XmlHelpers.GetAttribute(type, "n");
            dr["ParentID"] = parentId.ToUpper();
            dr["Comment"] = "";
            dt.Rows.Add(dr);
        }
    }
}
