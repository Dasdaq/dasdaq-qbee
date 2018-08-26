using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Dasdaq.Qbee.Web.Models;

namespace Dasdaq.Qbee.Web.Controllers
{
    [Route("api/[controller]")]
    public class CurrencyController : Controller
    {
        [HttpGet]
        public async Task<object> Get(
            [FromServices] IConfiguration config)
        {
            var text = System.IO.File.ReadAllText("currency.json");
            var ret = JsonConvert.DeserializeObject<IEnumerable<Currency>>(text);
            var sells_rows = await RequestTableAsync<TradeTableRow>(config["chain:host"], config["chain:contract_account"], "sellrecord");
            var buys_rows = await RequestTableAsync<TradeTableRow>(config["chain:host"], config["chain:contract_account"], "buyrecord");
            foreach (var x in ret)
            {
                var lst = new List<Trade>();
                lst.AddRange(sells_rows.rows
                    .Where(y => y.asset.Split(' ')[1] == x.id)
                    .Select(y => new Trade
                {
                    Account = y.account,
                    Asset = y.asset,
                    Eos = y.total_eos / 1000.0,
                    Type = TradeType.Sell
                }));
                x.Sells = lst;
            }
            foreach (var x in ret)
            {
                var lst = new List<Trade>();
                lst.AddRange(buys_rows.rows
                    .Where(y => y.asset.Split(' ')[1] == x.id)
                    .Select(y => new Trade
                {
                    Account = y.account,
                    Asset = y.asset,
                    Eos = y.total_eos / 1000.0,
                    Type = TradeType.Buy
                }));
                x.Buys = lst;
            }
            return ret;
        }

        private async Task<Table<T>> RequestTableAsync<T>(string host, string contractAccount, string table)
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(host) })
            using (var response = await client.PostAsync("/v1/chain/get_table_rows", new StringContent(GenerateRequestBody(table, contractAccount), Encoding.UTF8, "application/json")))
            {
                var text = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Table<T>>(text);
            }
        }

        private string GenerateRequestBody(string table, string contractAccount)
        {
            const string requestBodyTemplate = "{\"code\":\"{CONTRACT_ACCOUNT}\",\"scope\":\"{CONTRACT_ACCOUNT}\",\"table\":\"{TABLE}\",\"json\":true}";
            return requestBodyTemplate.Replace("{CONTRACT_ACCOUNT}", contractAccount).Replace("{TABLE}", table);
        }
    }
}
