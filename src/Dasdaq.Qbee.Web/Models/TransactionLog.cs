using System;

namespace Dasdaq.Qbee.Web.Models
{
    public class Order
    {
        public long Id { get; set; }

        public string Owner { get; set; }

        public OrderAsset Bid { get; set; }

        public OrderAsset Ask { get; set; }

        public long Timestamp { get; set; }
    }

    public class OrderAsset
    {
        public string Quantity { get; set; }

        public string Contract { get; set; }
    }
}
