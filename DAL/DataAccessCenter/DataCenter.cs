using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace GrabbingParts.DAL.DataAccessCenter
{
    public static class DataCenter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WXH");
        private static string connectionString = ConfigurationManager.ConnectionStrings["WXH"].ToString();
        private static int sqlCommandTimeout = Int32.Parse(ConfigurationManager.AppSettings["SqlCommandTimeout"]); //180s
        private static Object obj = new Object();
        private static Object obj1 = new Object();
        private static Object obj2 = new Object();
        private static Object obj3 = new Object();
        private static Object obj4 = new Object();
        private static Object obj5 = new Object();
        private static Object obj6 = new Object();

        public static void ExecuteTransaction(DataTable productSpecDataTable,
            DataTable manufacturerDataTable, DataTable productInfoDataTable, DataTable partAddressDataTable)
        {
            lock(obj)
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = conn.CreateCommand();

                SqlTransaction transaction = null;
                transaction = conn.BeginTransaction();
                command.Connection = conn;
                command.CommandTimeout = sqlCommandTimeout; //180s
                command.Transaction = transaction;

                try
                {
                    log.InfoFormat("插入[产品规格]记录数: {0}", productSpecDataTable.Rows.Count);
                    InsertDataToProductSpecTable(conn, transaction, productSpecDataTable);

                    int manufacturerCount = manufacturerDataTable.Rows.Count;
                    if (manufacturerCount != 0)
                    {
                        log.InfoFormat("插入[厂家资料]记录数: {0}", manufacturerDataTable.Rows.Count);
                        InsertDataToManufacturer(conn, transaction, manufacturerDataTable);
                    }                    

                    log.InfoFormat("插入[产品资料]记录数: {0}", productInfoDataTable.Rows.Count);
                    InsertDataToProductInfo(conn, transaction, productInfoDataTable);

                    log.InfoFormat("插入[零件地址_digikey]记录数: {0}", partAddressDataTable.Rows.Count);
                    InsertDataToPartAddress(conn, transaction, partAddressDataTable);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    transaction.Rollback();
                }
            }
        }

        public static void InsertDataToCategory(List<DataTable> categoryDataTables)
        {
            lock(obj2)
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = conn.CreateCommand();

                SqlTransaction transaction = null;
                transaction = conn.BeginTransaction();
                command.Connection = conn;
                command.Transaction = transaction;

                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        log.InfoFormat("插入第{0}层[产品分类]记录数: {1}", i, categoryDataTables[i].Rows.Count);
                        InsertDataForEachCategory(conn, transaction, categoryDataTables[i]);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    transaction.Rollback();
                }
            }            
        }

        private static void InsertDataForEachCategory(SqlConnection conn, SqlTransaction transaction, DataTable dt)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.DestinationTableName = "dbo.[产品分类]";//目标表，就是说您将要将数据插入到哪个表中去
                bulkCopy.ColumnMappings.Add("GUID", "GUID");//数据源中的列名与目标表的属性的映射关系
                bulkCopy.ColumnMappings.Add("Name", "名称");
                bulkCopy.ColumnMappings.Add("ParentID", "父ID");
                bulkCopy.ColumnMappings.Add("Comment", "备注");

                //bulkCopy.BatchSize = 3;
                Stopwatch stopwatch = new Stopwatch();//跑表，该类可以进行时间的统计

                stopwatch.Start();//跑表开始

                bulkCopy.WriteToServer(dt);//将数据源数据写入到目标表中

                log.InfoFormat("插入数据所用时间:{0}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public static void InsertDataToProductSpecTable(SqlConnection conn, SqlTransaction transaction, DataTable dt)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.DestinationTableName = "dbo.[产品规格]";//目标表，就是说您将要将数据插入到哪个表中去
                bulkCopy.ColumnMappings.Add("GUID", "GUID");//数据源中的列名与目标表的属性的映射关系
                bulkCopy.ColumnMappings.Add("PN", "产品编号");
                bulkCopy.ColumnMappings.Add("Name", "规格名称");
                bulkCopy.ColumnMappings.Add("Content", "规格内容");

                Stopwatch stopwatch = new Stopwatch();//跑表，该类可以进行时间的统计

                stopwatch.Start();//跑表开始

                bulkCopy.WriteToServer(dt);//将数据源数据写入到目标表中

                log.InfoFormat("插入[产品规格]数据所用时间:{0}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public static void InsertDataToSupplier(DataTable dt)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();

            SqlTransaction transaction = null;
            transaction = conn.BeginTransaction();
            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.DestinationTableName = "dbo.[供应商资料]";//目标表，就是说您将要将数据插入到哪个表中去
                    bulkCopy.ColumnMappings.Add("GUID", "GUID");//数据源中的列名与目标表的属性的映射关系
                    bulkCopy.ColumnMappings.Add("Supplier", "Supplier");
                    bulkCopy.ColumnMappings.Add("WebUrl", "WebUrl");

                    Stopwatch stopwatch = new Stopwatch();//跑表，该类可以进行时间的统计

                    stopwatch.Start();//跑表开始

                    bulkCopy.WriteToServer(dt);//将数据源数据写入到目标表中

                    log.InfoFormat("插入[供应商资料]数据所用时间:{0}ms", stopwatch.ElapsedMilliseconds);
                }

                transaction.Commit();
            }
            catch(Exception ex)
            {
                log.Error(ex);
                transaction.Rollback();
            }
        }

        public static void InsertDataToManufacturer(SqlConnection conn, SqlTransaction transaction, DataTable dt)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.DestinationTableName = "dbo.[厂家资料]";//目标表，就是说您将要将数据插入到哪个表中去
                bulkCopy.ColumnMappings.Add("GUID", "GUID");//数据源中的列名与目标表的属性的映射关系
                bulkCopy.ColumnMappings.Add("Manufacturer", "Manufacturer");

                Stopwatch stopwatch = new Stopwatch();//跑表，该类可以进行时间的统计

                stopwatch.Start();//跑表开始

                bulkCopy.WriteToServer(dt);//将数据源数据写入到目标表中

                log.InfoFormat("插入[厂家资料]数据所用时间:{0}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public static void InsertDataToProductInfo(SqlConnection conn, SqlTransaction transaction, DataTable dt)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.DestinationTableName = "dbo.[产品资料]";//目标表，就是说您将要将数据插入到哪个表中去
                bulkCopy.ColumnMappings.Add("GUID", "GUID");//数据源中的列名与目标表的属性的映射关系
                bulkCopy.ColumnMappings.Add("PN", "产品编号");
                bulkCopy.ColumnMappings.Add("SupplierPN", "SupplierPN");
                bulkCopy.ColumnMappings.Add("Manufacturer", "制造商");
                bulkCopy.ColumnMappings.Add("ManufacturerID", "制造商ID");
                bulkCopy.ColumnMappings.Add("Description", "描述");
                bulkCopy.ColumnMappings.Add("Packing", "包装");
                bulkCopy.ColumnMappings.Add("StandardPacking", "标准包装");
                bulkCopy.ColumnMappings.Add("Type1", "类别1GUID");
                bulkCopy.ColumnMappings.Add("Type2", "类别2GUID");
                bulkCopy.ColumnMappings.Add("Type3", "类别3GUID");
                bulkCopy.ColumnMappings.Add("DatasheetsUrl", "DatasheetsUrl");
                bulkCopy.ColumnMappings.Add("ImageUrl", "相片Url");
                bulkCopy.ColumnMappings.Add("ZoomImageUrl", "相片缩略图Url");

                Stopwatch stopwatch = new Stopwatch();//跑表，该类可以进行时间的统计

                stopwatch.Start();//跑表开始

                bulkCopy.WriteToServer(dt);//将数据源数据写入到目标表中
                
                log.InfoFormat("插入[产品资料]数据所用时间:{0}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public static void InsertDataToPartAddress(SqlConnection conn, SqlTransaction transaction, DataTable dt)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.DestinationTableName = "dbo.[零件地址_digikey]";
                bulkCopy.ColumnMappings.Add("GUID", "GUID");
                bulkCopy.ColumnMappings.Add("ManufacturerPN", "ManufacturerPN");
                bulkCopy.ColumnMappings.Add("SupplierPN", "SupplierPN");
                bulkCopy.ColumnMappings.Add("PartUrl", "PartUrl");

                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                bulkCopy.WriteToServer(dt);

                log.InfoFormat("插入[零件地址_digikey]数据所用时间:{0}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public static void DeleteSpecialWidgetFromDatabase(string widgetGuid)
        {
            lock (obj1)
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = conn.CreateCommand();

                SqlTransaction transaction = null;
                transaction = conn.BeginTransaction();
                command.Connection = conn;
                command.Transaction = transaction;

                try
                {
                    //Step 1
                    command.CommandText = string.Format("delete from [产品规格] where [产品编号] in (select [产品编号] from [产品资料] where [类别2GUID] = '{0}')", widgetGuid);
                    command.ExecuteNonQuery();

                    //Step 2
                    command.CommandText = string.Format("delete from [厂家资料] where [GUID] in (select [制造商ID] from [产品资料] where [类别2GUID] = '{0}' and [制造商ID] not in(select [制造商ID] from [产品资料]	where [类别2GUID] != '{0}'))", widgetGuid);
                    command.ExecuteNonQuery();

                    //Step 3
                    command.CommandText = string.Format("delete from [产品资料] where [类别2GUID] = '{0}'", widgetGuid);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    transaction.Rollback();
                }
            }
        }

        public static void InsertDataToProductErrorRecord(DataTable dt)
        {
            lock(obj3)
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = conn.CreateCommand();

                SqlTransaction transaction = null;
                transaction = conn.BeginTransaction();
                command.Connection = conn;
                command.Transaction = transaction;

                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = "dbo.[产品错误记录]";
                        bulkCopy.ColumnMappings.Add("CreareDate", "CreareDate");
                        bulkCopy.ColumnMappings.Add("PN", "产品编号");
                        bulkCopy.ColumnMappings.Add("Url", "Url");

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        bulkCopy.WriteToServer(dt);

                        log.InfoFormat("插入[产品错误记录]数据所用时间:{0}ms", stopwatch.ElapsedMilliseconds);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    transaction.Rollback();
                }
            }            
        }

        public static XElement GetPartInfoFromDatabase(string query)
        {
            lock(obj5)
            {
                XElement result = new XElement("r");
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = conn.CreateCommand();
                command.Connection = conn;

                try
                {
                    //command.CommandText = "select a.ManufacturerPN, a.SupplierPN, a.PartUrl, b.GUID as KeyPNGUID, b.产品编号 as PN, b.制造商ID as ManufacturerGUID, b.制造商 as Manufacturer from [零件地址_digikey] a, [产品资料] b where a.SupplierPN = b.SupplierPN";
                    command.CommandText = query;
                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        while (reader.Read())
                        {
                            result.Add(new XElement("p", new XAttribute("manpn", reader["ManufacturerPN"]),
                                                         new XAttribute("suppn", reader["SupplierPN"]),
                                                         new XAttribute("url", reader["PartUrl"]),
                                                         new XAttribute("keypnguid", reader["KeyPNGUID"].ToString()),
                                                         new XAttribute("pn", reader["PN"]),
                                                         new XAttribute("manguid", reader["ManufacturerGUID"].ToString()),
                                                         new XAttribute("man", reader["Manufacturer"])));
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return null;
                }
            }            
        }

        public static XElement GetSupplierPartNumberFromDatabase(string query)
        {
            lock (obj6)
            {
                XElement result = new XElement("r");
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = conn.CreateCommand();
                command.Connection = conn;
                command.CommandTimeout = sqlCommandTimeout; //180s

                try
                {
                    command.CommandText = query;
                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        while (reader.Read())
                        {
                            result.Add(new XElement("p", new XAttribute("suppn", reader["SupplierPN"])));
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return null;
                }
            }
        }

        public static string GetSupplierGuid(string supplier)
        {
            string result = "";
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            command.Connection = conn;

            try
            {
                command.CommandText = string.Format("select [GUID] from  [供应商资料] where Supplier='{0}'", supplier);
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    while (reader.Read())
                    {
                        result = reader["GUID"].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return "";
            }
        }

        public static void InsertDataToGrabbingResult(DataTable dt)
        {
            lock (obj4)
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = conn.CreateCommand();
                SqlTransaction transaction = null;
                transaction = conn.BeginTransaction();
                command.Connection = conn;
                command.Transaction = transaction;

                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = "dbo.[抓取结果_digikey]";
                        bulkCopy.ColumnMappings.Add("GUID", "GUID");
                        bulkCopy.ColumnMappings.Add("CreateDate", "CreateDate");
                        bulkCopy.ColumnMappings.Add("KeyPNGUID", "KeyPNGUID");
                        bulkCopy.ColumnMappings.Add("KeyPN", "KeyPN");
                        bulkCopy.ColumnMappings.Add("SupplierGUID", "SupplierGUID");
                        bulkCopy.ColumnMappings.Add("Supplier", "Supplier");
                        bulkCopy.ColumnMappings.Add("PN", "PN");
                        bulkCopy.ColumnMappings.Add("ManufacturerGUID", "ManufacturerGUID");
                        bulkCopy.ColumnMappings.Add("Manufacturer", "Manufacturer");
                        bulkCopy.ColumnMappings.Add("QtyStr", "QtyStr");
                        bulkCopy.ColumnMappings.Add("Qty", "Qty");
                        bulkCopy.ColumnMappings.Add("QtyMinStr", "QtyMinStr");
                        bulkCopy.ColumnMappings.Add("QtyMin", "QtyMin");
                        bulkCopy.ColumnMappings.Add("QtyMax", "QtyMax");
                        bulkCopy.ColumnMappings.Add("UnitPriceStr", "UnitPriceStr");
                        bulkCopy.ColumnMappings.Add("UnitPrice", "UnitPrice");
                        bulkCopy.ColumnMappings.Add("MoneyType", "MoneyType");
                        bulkCopy.ColumnMappings.Add("StockStr", "StockStr");
                        bulkCopy.ColumnMappings.Add("Stock", "Stock");
                        bulkCopy.ColumnMappings.Add("Packing", "Packing");
                        bulkCopy.ColumnMappings.Add("Descript", "Descript");

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        bulkCopy.WriteToServer(dt);

                        log.InfoFormat("插入[抓取结果_digikey]数据所用时间:{0}ms", stopwatch.ElapsedMilliseconds);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    transaction.Rollback();
                }
            }
        }
    }
}