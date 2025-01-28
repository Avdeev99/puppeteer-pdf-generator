using PuppeteerSharp;

namespace PuppeteerPdfGenerator.Api.Models;

public class GeneratePdfOptions
{
    public string ContentHtml { get; set; }

    public PdfOptions PdfOptions { get; set; }
}
