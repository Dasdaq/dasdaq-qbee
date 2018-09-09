using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Text;
using System.Linq;
using Pomelo.Data.InfluxDB;

namespace Dasdaq.Qbee.PingTxNode
{
    class NodeQuality
    {
        public string Host { get; set; }

        public long Latency { get; set; }
    }

    class Program
    {
        const string Url = "https://validate.eosnation.io";
        const string Endpoint = "/report-endpoints.txt";
        const string InfluxDB = "Server=127.0.0.1;Database=dasdaq;";

        static async Task<IEnumerable<string>> GetTransactionNodesAsync()
        {
            Console.WriteLine("Pulling the tx node list...");
            using (var client = new HttpClient { BaseAddress = new Uri(Url) })
            using (var resposne = await client.GetAsync(Endpoint))
            {
                var text = await resposne.Content.ReadAsStringAsync();
                return text
                    .Split('\n')
                    .Where(x => x.Contains("http://") || x.Contains("https://"))
                    .Where(x => x.Contains(" "))
                    .Select(x => x.Split(' ')[1])
                    .ToList();
            }
        }

        static async Task<IEnumerable<NodeQuality>> PingAsync()
        {
            var nodes = await GetTransactionNodesAsync();
            Console.WriteLine($"Ping { nodes.Count() } nodes...");
            var ret = new ConcurrentBag<NodeQuality>();
            foreach(var g in GroupNodes(nodes))
            {
                await Task.WhenAll(g.Select(x => Task.Factory.StartNew(async () => {
                    var result = await PingSingleAsync(x);
                    ret.Add(result);
                })));
            }
            return ret;
        }

        static IEnumerable<IEnumerable<string>> GroupNodes(IEnumerable<string> src)
        {
            var cnt = 0;
            while(cnt < src.Count())
            {
                var nodes = src.Skip(cnt).Take(10);
                cnt += nodes.Count();
                yield return nodes;
            }
        }
        
        static async Task<NodeQuality> PingSingleAsync(string host)
        {
            using (var ping = new Ping())
            {
                host = host.Replace("http://", "").Replace("https://", "");
                var result = await ping.SendPingAsync(host, 5000);
                return new NodeQuality
                {
                    Host = host,
                    Latency = result.RoundtripTime
                };
            }
        }

        static async Task UploadResultAsync(IEnumerable<NodeQuality> result)
        {
            using (var conn = new InfluxConnection(InfluxDB))
            {
                var time = DateTime.UtcNow;
                conn.Open();
                var parameters = new List<InfluxParameter>();
                var row = 0;
                var commandBuilder = new StringBuilder();
                foreach (var x in result)
                {
                    commandBuilder.AppendLine($"INSERT MonTxNodes,host=@p{row}_0 latency=@p{row}_1 @p{row}_2");
                    parameters.Add(new InfluxParameter($"p{row}_0", x.Host, InfluxParameterType.Tag));
                    parameters.Add(new InfluxParameter($"p{row}_1", x.Latency, InfluxParameterType.Field));
                    parameters.Add(new InfluxParameter($"p{row}_2", time, InfluxParameterType.Timestamp));
                    ++row;
                }
                using (var cmd = new InfluxCommand(commandBuilder.ToString().TrimEnd('\n').TrimEnd('\r'), conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        static async Task DoWork()
        {
            while(true)
            {
                var result = await PingAsync();
                await UploadResultAsync(result);
                Console.WriteLine($"{result.Count()} nodes responded.");
                await Task.Delay(10000);
            }
        }

        static void Main(string[] args)
        {
            DoWork().Wait();
        }
    }
}
