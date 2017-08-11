using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApiCalls
{
    public static class Helper
    {
        public static List<string> GetCashiers()
        {
            string URL_Domain = "http://walletapi20170810041706.azurewebsites.net/api/";
            var results = new List<string>();
            try
            {
                string Url = URL_Domain + "Cashier";

                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                request.Method = "GET";
                request.ContentType = "application/json";
                // Get response  
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    Stream responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        // get the response as text
                        string responseText = reader.ReadToEnd();

                        // convert from text 
                        var cahierResults = JsonConvert.DeserializeObject<List<Cashier>>(responseText);
                        results = cahierResults.Select(c => c.EmailAddress).ToList();
                        
                    }
                }
            }

            catch (Exception es)
            {
            }

            return results;
        }
    }

    public class Cashier
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; }
    }
}
