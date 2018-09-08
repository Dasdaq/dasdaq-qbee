using System.Collections.Generic;

namespace Dasdaq.Qbee.KdataRunner.Models
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
        public string bid { get; set; }
        public string ask { get; set; }
    }
}
