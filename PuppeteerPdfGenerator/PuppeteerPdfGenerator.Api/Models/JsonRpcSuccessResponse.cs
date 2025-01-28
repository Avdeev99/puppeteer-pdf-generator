using System.Text.Json.Serialization;

namespace PuppeteerPdfGenerator.Api.Models
{
    public class JsonRpcSuccessResponse
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
