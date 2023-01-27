using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleSheetAndCsharp.Entities
{
    public class MarketSnapshot
    {
        public Dictionary<string, List<PortfolioItem>> Portfolios { get; set; }
    }
}
