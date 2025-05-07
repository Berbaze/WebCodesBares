using Newtonsoft.Json;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebCodesBares.Data.Service
{
    public class SynologyAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _synologyBaseUrl = "https://Mikroplus.DSCloud.me:1998/";
        

        public SynologyAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> LoginAsync(string username, string password)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["api"] = "SYNO.API.Auth";
            query["method"] = "Login";
            query["version"] = "6";
            query["account"] = username;
            query["passwd"] = password;
            query["session"] = "FileStation";
            query["format"] = "sid";

            var url = $"{_synologyBaseUrl}/webapi/auth.cgi?{query}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<SynologyLoginResponse>(json);

            if (result.success)
            {
                return result.data.sid; 
            }
            throw new Exception("Login failed Synology: " + json);

        }
    }
}
