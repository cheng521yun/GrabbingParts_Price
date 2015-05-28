using GrabbingParts.BLL.ScraperLibrary;
using System.Configuration;
using GrabbingParts.BLL;
using GrabbingParts.Model;
using GrabbingParts.BLL.Price;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrabbingParts
{
    static class Program
    {
        static void Main(string[] args)
        {
            /*
            if (ConfigurationManager.AppSettings["GetScrapeResult"] == "true")
            {
                ResultScraper resultScraper = new DigikeyResultScraper();
                resultScraper.ScrapeResult();
            }
            else
            {
                Scraper scraper = new DigikeyScraper();
                scraper.Scrape();
            }      
      */
            GetPrice();
            
        }

        public static void GetPrice()
        {
            string PN = "m";//for testing

            BLL.Price.Price price = new BLL.Price.Price();
            List<List<PriceResult>> priceList = price.GetPrice(PN);
        }
    }
}