using System;

namespace Dasdaq.Qbee.KdataRunner.Models
{
    public class TransactionLog
    {
        public long Id { get; set; }

        public string Bidder { get; set; }

        public string Asker { get; set; }

        public TransactionAsset Bid { get; set; }

        public TransactionAsset Ask { get; set; }

        public long Timestamp { get; set; }
    }

    public class TransactionAsset
    {
        public string Quantity { get; set; }

        public string Contract { get; set; }
    }

}
