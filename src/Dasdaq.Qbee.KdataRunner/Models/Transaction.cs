using System;

namespace Dasdaq.Qbee.KdataRunner.Models
{
    public class Transaction
    {
        public string catalog { get; set; }

        public double price { get; set; }

        public double count { get; set; }

        public string user { get; set; }

        public string user2 { get; set; }

        public DateTime utcTime { get; set; } = DateTime.UtcNow;
    }
}
