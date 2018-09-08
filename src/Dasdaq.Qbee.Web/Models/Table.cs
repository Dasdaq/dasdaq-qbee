using System.Collections.Generic;

namespace Dasdaq.Qbee.Web.Models
{
    public class Table<T>
    {
        public IEnumerable<T> rows { get; set; }
        public bool more { get; set; } // TODO: Need investigate how to use pagination
    }

    public class TransactionLogRow
    {
        public ulong id { get; set; }
        public ulong timestamp { get; set; }
        public string seller { get; set; }
        public string buyer { get; set; }
        public string asset { get; set; }
        public ulong total_eos { get; set; }
        public double per { get; set; }
    }

    public class TradeTableRow
    {
        public ulong id { get; set; }
        public string bid { get; set; }
        public string ask { get; set; }
        public string account { get; set; }
    }
}
