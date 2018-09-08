using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Timers;
using System.Linq;
using Dasdaq.Qbee.KdataRunner.Models;
using Newtonsoft.Json;

namespace Dasdaq.Qbee.KdataRunner
{
    class Program
    {
        static HttpClient chain_client;
        static HttpClient api_client;
        static Config config;
        static Timer timer;
        const string getTableRowsEndpoint = "/v1/chain/get_table_rows";
        const string postKdataEndpoint = "/api/Candlestick";
        const string postTransactionEndpoint = "/api/Transaction";
        const string requestBodyTemplate = "{\"code\":\"{CONTRACT_ACCOUNT}\",\"scope\":\"{CONTRACT_ACCOUNT}\",\"table\":\"txlog\",\"json\":true}";

        static void Main(string[] args)
        {
            InitConfig();
            InitHttpClient();
            InitTimer();
            while(true)
            {
                Console.Read();
            }
        }

        static void InitConfig()
        {
            var text = File.ReadAllText("config.json");
            config = JsonConvert.DeserializeObject<Config>(text);
        }

        static void InitHttpClient()
        {
            chain_client = new HttpClient() { BaseAddress = new Uri(config.Chain.Host) };
            api_client = new HttpClient() { BaseAddress = new Uri(config.Log.Api) };
        }

        static string GenerateRequestBody(string issuer)
        {
            return requestBodyTemplate
                .Replace("{CONTRACT_ACCOUNT}", config.Chain.ContractAccount)
                .Replace("{ISSUER_ACCOUNT}", issuer);
        }

        static async Task<Table<TransactionLogRow>> RequestTableAsync(ChainToken token)
        {
            using (var response = await chain_client.PostAsync(getTableRowsEndpoint, new StringContent(GenerateRequestBody(token.Issuer), Encoding.UTF8, "application/json")))
            {
                var text = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Table<TransactionLogRow>>(text);
            }
        }

        static IEnumerable<TransactionLog> MapTableToTransactionModel(IEnumerable<TransactionLogRow> rows)
        {
            foreach(var x in rows)
            {
                yield return new TransactionLog
                {
                    Id = (long)x.id,
                    AssetAmount = Convert.ToDouble(x.asset.Split(' ')[0]),
                    AssetSymbol = x.asset.Split(' ')[1],
                    TotalEos = x.total_eos / 10000.0,
                    Buyer = x.buyer,
                    Seller = x.seller,
                    Per = x.per,
                    Time = new DateTime((long)x.timestamp / 1000, DateTimeKind.Utc)
                };
            }
        }

        static Upload<Candlestick> GenerateCandlestickData(IEnumerable<TransactionLog> logs)
        {
            var ret = new Upload<Candlestick>();
            ret.values = new List<Candlestick>();
            foreach (var x in logs)
            {
                (ret.values as List<Candlestick>).Add(new Candlestick
                {
                    catalog = config.Log.CatalogPrefix + x.AssetSymbol,
                    price = x.Per
                });
            }
            return ret;
        }

        static Upload<Transaction> GenerateTransactionData(IEnumerable<TransactionLog> logs)
        {
            var ret = new Upload<Transaction>();
            ret.values = new List<Transaction>();
            foreach (var x in logs)
            {
                (ret.values as List<Transaction>).Add(new Transaction
                {
                    catalog = config.Log.CatalogPrefix + x.AssetSymbol,
                    price = x.TotalEos,
                    count = x.AssetAmount,
                    user = x.Buyer,
                    user2 = x.Seller
                });
            }
            return ret;
        }

        static async Task UploadLogsAsync(Upload<Transaction> transactions, Upload<Candlestick> candlesticks)
        {
            using (var response = await api_client.PostAsync(postKdataEndpoint, new StringContent(JsonConvert.SerializeObject(candlesticks), Encoding.UTF8, "application/json")))
            {
            }

            using (var response = await api_client.PostAsync(postTransactionEndpoint, new StringContent(JsonConvert.SerializeObject(transactions), Encoding.UTF8, "application/json")))
            {
            }
        }

        static void InitTimer()
        {
            timer = new Timer(1000 * 60);
            timer.Elapsed += async (object sender, ElapsedEventArgs e) => {
                foreach(var x in config.Chain.Tokens)
                {
                    Console.WriteLine("Requesting table rows...");
                    var response = await RequestTableAsync(x);

                    Console.WriteLine($"{response.rows.Count()} rows found.");
                    var transactions = MapTableToTransactionModel(response.rows);

                    Console.WriteLine("Uploading data...");
                    await UploadLogsAsync(GenerateTransactionData(transactions), GenerateCandlestickData(transactions));

                    Console.WriteLine("Timer elapse finished.");
                }
            };
            timer.Start();
        }
    }
}
