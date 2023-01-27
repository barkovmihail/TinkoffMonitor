using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace TinkoffMonitor.Entities
{
    public class Settings
    {
        public string TinkoffToken { get; set; }

        public string SpreadsheetId { get; set; }
    }
}

