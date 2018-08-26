using System;

namespace Dasdaq.Qbee.KdataRunner.Models
{
    public class Candlestick
    {
        public double price { get; set; }
        public DateTime utcTime { get; set; } = DateTime.UtcNow;
        public string catalog { get; set; }
    }
}
