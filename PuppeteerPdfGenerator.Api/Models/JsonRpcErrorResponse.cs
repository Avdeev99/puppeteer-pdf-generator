using System.Text.Json.Serialization;

namespace PuppeteerPdfGenerator.Api.Models
{
    public class JsonRpcErrorResponse
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("error")]
        public JsonRpcError Error { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class JsonRpcError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}