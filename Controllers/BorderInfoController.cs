using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EmisTechTest.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmisTechTest.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BorderInfoController : Controller
    {
        
        //input could be borderinfo?currency=GBP
        [HttpGet]
        public async Task<string> GetCountriesList()
        {

            string currency = Request.Query["currency"];
            String rawJsonCurrency;
            String rawJsonAll;
            using (var webClient = new WebClient())
            {
                rawJsonCurrency = webClient.DownloadString("https://restcountries.eu/rest/v2/currency/" + currency);
                rawJsonAll = webClient.DownloadString("https://restcountries.eu/rest/v2/all");
            }
                /*TODO: task 1 
                / a list of countries which use that currency.
                */
                List<CountryModel> currList = JsonConvert.DeserializeObject<List<CountryModel>>(rawJsonCurrency);
                List<string> sharedCurrList = new List<string>();
                List<string> sharedBorderList = new List<string>(); //this is for step 2.
                for (int i = 0; i < currList.Count; i++)
                {
                    sharedCurrList.Add(currList[i].Alpha3Code + " - " + currList[i].Name); //alpha3code
                    //add borders to sharedborderslist for step 2.
                    for (int j = 0; j < currList[i].Borders.Count; j++)
                    {
                        sharedBorderList.Add(currList[i].Borders[j]); // sharedborderlist now contains all the countries which border countries using this currency.
                    }
                }
                /*TODO: task 2
                / a list of countries which border the countries in 1 but do not use the currency.
                */
                List<CountryModel> fullList = JsonConvert.DeserializeObject<List<CountryModel>>(rawJsonAll);
                List<string> borderNoCurrency = new List<string>();
                for (int i = 0; i < fullList.Count; i++)
                {
                    //loop through sharedborderlist and check if currency matches.
                    for (int j = 0; j < sharedBorderList.Count; j++)
                    {
                        //if alpha code matches then we need to check currency.
                        if (fullList[i].Alpha3Code == sharedBorderList[j])
                        {
                            //check all currencies for that country.
                            for (int n = 0; n < fullList[i].Currencies.Count; n++)
                            {
                                //if currency is not the same
                                if (fullList[i].Currencies[n].Code != currency)
                                {
                                    borderNoCurrency.Add(sharedBorderList[j] + " - " + fullList[i].Name);
                                }
                            }
                        }
                    }
                }

                //remove duplicates.
                sharedCurrList = sharedCurrList.Distinct().ToList();
                borderNoCurrency = borderNoCurrency.Distinct().ToList();

                //reconstruct json
                JObject rss =
                        new JObject(
                            new JProperty("countries which use currency", new JArray(sharedCurrList)),
                            new JProperty("Countries which border the countries in 1 but do not use the currency", new JArray(borderNoCurrency)));
                string output = rss.ToString();
                return output;
            
        }
    }
}