using System.Collections.Generic;

namespace Dasdaq.Qbee.KdataRunner.Models
{
    public class Table<T>
    {
        public IEnumerable<T> rows { get; set; }
        public bool more { get; set; } // TODO: Need investigate how to use pagination
    }
}
