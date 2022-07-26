using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERSRParserNET
{
    public static class Servises
    {
        public static async Task<string> GetResponseHTML(string _url)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(_url);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public static async Task<string> PostResponseHTML(string _url, Dictionary<string, string> _headers)
        {
            HttpClient client = new HttpClient();

            var content = new FormUrlEncodedContent(_headers);

            var response = await client.PostAsync(_url, content);

            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
    }
}
