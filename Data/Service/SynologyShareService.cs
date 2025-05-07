using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Org.BouncyCastle.Math.EC;



namespace WebCodesBares.Data.Service
{
    public class SynologyShareService
    {
        private readonly HttpClient _httpClient;
        private readonly string _synologyBaseUrl = "https://Mikroplus.DSCloud.me:1998/";
        private readonly ILogger<SynologyShareService> _logger;

        public SynologyShareService(HttpClient httpClient, ILogger<SynologyShareService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> CreateShareLinkAsync(string sid, string path, int expireInDays = 7)
        {
            var parameters = new Dictionary<string, string>
        {
            { "api", "SYNO.FileStation.Sharing" },
            { "method", "create" },
            { "version", "3" },
            { "path", path },
            { "expire_in", expireInDays.ToString() },
            { "enable_password", "false" },
            { "_sid", sid }
        };

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync($"{_synologyBaseUrl}/webapi/entry.cgi", content);

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("🔍 Réponse Synology JSON : {json}", json);

            var result = JsonConvert.DeserializeObject<SynologyShareResponse>(json);

            if (result != null && result.success && result.data?.links?.Count > 0)
            {
                return result.data.links[0].url;
            }

            // 🔴 Logging complet si erreur
            _logger.LogWarning("💣 Échec création lien Synology pour {Path}. Réponse : {json}", path, json);

            // 🛑 Évite 'no return'
            throw new Exception($"Échec création de lien : {json}");
        }
    }
}
