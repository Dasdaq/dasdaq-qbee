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
            foreach (var x in ret)
            {
                var sells_rows = await RequestTableAsync<TradeTableRow>(config["chain:host"], config["chain:contract_account"], "sellorder", x.issuer);
                var buys_rows = await RequestTableAsync<TradeTableRow>(config["chain:host"], config["chain:contract_account"], "buyorder", x.issuer);
                
                x.Sells = sells_rows.rows
                    .Where(y => y.ask.Split(' ')[1] == x.id)
                    .Select(y => new Trade
                    {
                        Account = y.account,
                        Ask = y.ask,
                        Bid = y.bid,
                        Type = TradeType.Sell
                    })
                    .ToList();
                x.Buys = buys_rows.rows
                    .Where(y => y.ask.Split(' ')[1] == x.id)
                    .Select(y => new Trade
                    {
                        Account = y.account,
                        Ask = y.ask,
                        Bid = y.bid,
                        Type = TradeType.Buy
                    })
                    .ToList();
            }
            return ret;
        }

        private async Task<Table<T>> RequestTableAsync<T>(string host, string contractAccount, string table, string issuer)
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(host) })
            using (var response = await client.PostAsync("/v1/chain/get_table_rows", new StringContent(GenerateRequestBody(table, contractAccount, issuer), Encoding.UTF8, "application/json")))
            {
                var text = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Table<T>>(text);
            }
        }

        private string GenerateRequestBody(string table, string contractAccount, string issuer)
        {
            const string requestBodyTemplate = "{\"code\":\"{CONTRACT_ACCOUNT}\",\"scope\":\"{ISSUER}\",\"table\":\"{TABLE}\",\"json\":true}";
            return requestBodyTemplate.Replace("{CONTRACT_ACCOUNT}", contractAccount).Replace("{TABLE}", table).Replace("{ISSUER}", issuer);
        }
    }
}
