using System.Text.Json.Serialization;
using PuppeteerSharp;

namespace PuppeteerPdfGenerator.Api.Models
{
    public class JsonRpcRequest
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("params")]
        public GeneratePdfParams Params { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class GeneratePdfParams
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("pdfOptions")]
        public PdfOptions PdfOptions { get; set; }
    }
}