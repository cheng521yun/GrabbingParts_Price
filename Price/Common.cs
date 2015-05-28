using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Web;
namespace GrabbingParts.BLL.Price
{
    public class Common
    {
        /// <summary>
        /// remove all comments, script, style
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static HtmlDocument RemoveRedundancyCode(HtmlDocument doc)
        {
            foreach (var comment in doc.DocumentNode.SelectNodes("//comment()").ToArray())
                comment.Remove();
            foreach (var script in doc.DocumentNode.Descendants("script").ToArray())
                script.Remove();
            foreach (var style in doc.DocumentNode.Descendants("style").ToArray())
                style.Remove();

            return doc;
        }

        /// <summary>
        /// remove \r \t \n
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FormatString(string str)
        {
            string result = HttpUtility.HtmlDecode(str);
            result = result.Replace("\r", "").Replace("\n", "").Replace("\t", "")
                .Replace("¥", "").Replace("CNY", "").Replace("€","")
                .Replace(",", "").Replace("+", "")
                .Replace("$", "");         
            return result.Trim();
        }
        public static string FormatNumber(string str)
        {
            string result = FormatString(str);

            return string.IsNullOrEmpty(result) ? "0" : result;
        }
        public static string FormatCurrency(string currency)
        {
            string curr = "";
            switch (currency)
            {
                case "$": curr = "USD"; break;
                case "€": curr = "EUR"; break;
                case "¥": curr = "CNY"; break;
            }

            return curr;
        }

    }
}

