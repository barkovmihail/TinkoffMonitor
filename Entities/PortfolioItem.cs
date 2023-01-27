using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleSheetAndCsharp.Entities
{
    public class PortfolioItem
    {
        public string Ticker { get; set; }

        public int Count { get; set; }

        public double Price { get; set; }

        public PortfolioItem()
        {
        }

        public PortfolioItem(string ticker, int count, double price)
        {
            this.Ticker = ticker;
            this.Count = count;
            this.Price = price;
        }
    }
}
