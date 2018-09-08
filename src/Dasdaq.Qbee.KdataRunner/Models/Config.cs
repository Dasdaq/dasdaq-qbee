using System.Collections.Generic;

namespace Dasdaq.Qbee.KdataRunner.Models
{
    public class Config
    {
        public ChainConfig Chain { get; set; }

        public LogConfig Log { get; set; }
    }

    public class ChainConfig
    {
        public string Host { get; set; }

        public string ContractAccount { get; set; }

        public IEnumerable<ChainToken> Tokens { get; set; }
    }

    public class ChainToken
    {
        public string Token { get; set; }

        public string Issuer { get; set; }
    }

    public class LogConfig
    {
        public string CatalogPrefix { get; set; }

        public string Api { get; set; }
    }
}
