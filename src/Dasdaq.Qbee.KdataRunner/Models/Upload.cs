using System.Collections.Generic;

namespace Dasdaq.Qbee.KdataRunner.Models
{
    public class Upload<T>
    {
        public IEnumerable<T> values { get; set; }
    }
}
