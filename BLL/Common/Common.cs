﻿using System;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Data;
using GrabbingParts.DAL.DataAccessCenter;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace GrabbingParts.BLL.Common
{
    public static class Common
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WXH");

        public static HtmlDocument RetryRequest(string url, string partId = "")
        {
            try
            {
                HtmlWeb htmlWeb = new HtmlWeb();
                return Retry.Do(() => htmlWeb.Load(url), TimeSpan.FromSeconds(60));
            }
            catch (Exception ex)
            {
                InsertDataToProductErrorRecord(url, partId);

                log.Error("Error in RetryRequest, url: " + url, ex);
                return null;
            }
        }

        //函数说明：这里可以根据远程图片地址，获取到图片并存储到本地或者本地服务器中，也可以并发把图片存储到本地数据库中        
        /// <summary>
        /// 上传文件到FTP服务器中，并且获取到文件的字节流
        /// </summary>
        /// <param name="targetAddress">远程服务器文件地址(比如：http://192.168.1.0\/filename.dwg)</param>
        /// <param name="ftpServerAddress">FTP服务器地址，并且指定上传到FTP服务器文件名(比如：ftp://192.168.1.1\/filename.dwg)</param>
        /// <returns>返回远程文件的字节数组</returns>
        public static void UpFileToFTPAndGetFileBytes(string targetAddress, string ftpServerAddress)
        {
            WebClient client = new WebClient();//初始化web访问客户端
            try
            {
                string URL = @"" + targetAddress + "";
                int n = URL.LastIndexOf("/");
                string URLAddress = URL.Substring(0, n);
                WebRequest webRequest = WebRequest.Create(URLAddress);//创建web请求

                #region 创建ftp访问请求 并且设定FTP请求的相关属性，如：验证用户名和密码、发送到FTP中的命令等属性
                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(ftpServerAddress);//创建ftp访问请求
                ftpWebRequest.Credentials = new NetworkCredential("chengyun", "CY-0331_Wxh");
                ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Timeout = 10 * 1000;
                ftpWebRequest.ContentLength = 10000000;
                ftpWebRequest.Proxy = new WebProxy();
                Stream stream = ftpWebRequest.GetRequestStream();
                #endregion

                Stream str = client.OpenRead(URL);
                byte[] mbyte = new byte[10000000];
                int allmybyte = (int)mbyte.Length;
                int startmbyte = 0;
                while (allmybyte > 0)
                {
                    int m = str.Read(mbyte, startmbyte, allmybyte);
                    if (m == 0)
                        break;
                    stream.Write(mbyte, startmbyte, m);//向FTP服务器上载的数据流
                    startmbyte += m;
                    allmybyte -= m;
                }
                str.Close();
                stream.Close();
                stream.Dispose();
            }
            catch (WebException exp)
            {
                //log.Error(exp);
                log.InfoFormat("Error in UpFileToFTPAndGetFileBytes: {0}, {1}", targetAddress, ftpServerAddress);
            }
        }

        private static void InsertDataToProductErrorRecord(string url, string partId)
        {
            if(url != "" && partId != "")
            {
                DataTable productErrorRecordDataTable = new DataTable();
                productErrorRecordDataTable.Columns.Add("CreareDate", typeof(DateTime));
                productErrorRecordDataTable.Columns.Add("PN", typeof(string));
                productErrorRecordDataTable.Columns.Add("Url", typeof(string));

                if (partId.Length > 64)
                {
                    partId = partId.Substring(0, 64);
                }

                if (url.Length > 128)
                {
                    url = url.Substring(0, 128);
                }

                DataRow productErrorRecord = productErrorRecordDataTable.NewRow();
                productErrorRecord["CreareDate"] = DateTime.Now.Date;
                productErrorRecord["PN"] = partId;
                productErrorRecord["Url"] = url;
                productErrorRecordDataTable.Rows.Add(productErrorRecord);

                DataCenter.InsertDataToProductErrorRecord(productErrorRecordDataTable);
            }            
        }
    }
}
