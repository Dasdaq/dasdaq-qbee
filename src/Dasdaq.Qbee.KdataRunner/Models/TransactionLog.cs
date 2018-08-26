using System;

namespace Dasdaq.Qbee.KdataRunner.Models
{
    public class TransactionLog
    {
        public long Id { get; set; }

        public string Seller { get; set; }

        public string Buyer { get; set; }

        public double AssetAmount { get; set; }

        public string AssetSymbol { get; set; }

        public double TotalEos { get; set; }

        public double Per { get; set; }

        public DateTime Time { get; set; }
    }
}
