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

        public bool pin { get; set; }

        public IEnumerable<Trade> Sells { get; set; }

        public IEnumerable<Trade> Buys { get; set; }
    }

    public class Trade
    {
        public string Account { get; set; }

        public string Ask { get; set; }

        public string Bid { get; set; }

        public TradeType Type { get; set; }
    }
}
