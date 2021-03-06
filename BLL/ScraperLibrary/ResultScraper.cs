﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrabbingParts.BLL.ScraperLibrary
{
    public abstract class ResultScraper : IResultScraper
    {
        public bool ScrapeResult()
        {
            this.Scrape();
            return true;
        }

        public abstract void Scrape();
    }
}
