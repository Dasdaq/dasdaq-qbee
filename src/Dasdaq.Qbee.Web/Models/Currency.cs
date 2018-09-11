using System;
using System.Collections.Generic;

namespace Dasdaq.Qbee.Web.Models
{
    public enum TradeType
    {
        Sell,
        Buy
    }

    public class Currency
    {
        public string id { get; set; }

        public string description { get; set; }

        public string website { get; set; }

        public string issuer { get; set; }

        public bool pin { get; set; }

        public IEnumerable<Order> Orders { get; set; }
    }
}
