using System.Text.Json.Serialization;

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
        [JsonPropertyName("contentHtml")]
        public string ContentHtml { get; set; }

        [JsonPropertyName("pdfOptions")]
        public PdfOptions PdfOptions { get; set; }
    }

    public class PdfOptions
    {
        [JsonPropertyName("format")]
        public string Format { get; set; }

        [JsonPropertyName("printBackground")]
        public bool PrintBackground { get; set; }

        [JsonPropertyName("displayHeaderFooter")]
        public bool DisplayHeaderFooter { get; set; }

        [JsonPropertyName("margin")]
        public PdfMargin Margin { get; set; }

        [JsonPropertyName("headerTemplate")]
        public string HeaderTemplate { get; set; }

        [JsonPropertyName("footerTemplate")]
        public string FooterTemplate { get; set; }
    }

    public class PdfMargin
    {
        [JsonPropertyName("top")]
        public string Top { get; set; }

        [JsonPropertyName("bottom")]
        public string Bottom { get; set; }
    }
}