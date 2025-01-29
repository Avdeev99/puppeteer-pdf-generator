using System.Diagnostics;
using Polly;
using PuppeteerPdfGenerator.Api.Models;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace PuppeteerPdfGenerator.Api.Services;

public class PdfGeneratorService : IPdfGeneratorService
{
    private readonly ILogger<PdfGeneratorService> _logger;
    private readonly IPagePoolService _pagePoolService;
    private readonly IAsyncPolicy<byte[]> _retryPolicy;

    public PdfGeneratorService(ILogger<PdfGeneratorService> logger, IPagePoolService pagePoolService)
    {
        _logger = logger;
        _pagePoolService = pagePoolService;
        _retryPolicy = BuildRetryPolicy();
    }

    public async Task<byte[]> GeneratePdfAsync(GeneratePdfParams options, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Puppeteer PDF generation started.");

        var watcher = new Stopwatch();

        var result = await _retryPolicy.ExecuteAsync(async () =>
        {
            await using var page = await _pagePoolService.GetPageAsync(cancellationToken);

            try
            {
                if (!string.IsNullOrEmpty(options.ContentHtml))
                {
                    await page.SetContentAsync(
                        options.ContentHtml,
                        new NavigationOptions
                        {
                            WaitUntil = [WaitUntilNavigation.Networkidle0, WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded]
                        });
                }

                var puppeteerPdfOptions = new PuppeteerSharp.PdfOptions
                {
                    Format = GetPaperFormat(options.PdfOptions.Format),
                    PrintBackground = options.PdfOptions.PrintBackground,
                    DisplayHeaderFooter = options.PdfOptions.DisplayHeaderFooter,
                    MarginOptions = new MarginOptions
                    {
                        Top = options.PdfOptions.Margin.Top,
                        Bottom = options.PdfOptions.Margin.Bottom
                    },
                    HeaderTemplate = options.PdfOptions.HeaderTemplate,
                    FooterTemplate = options.PdfOptions.FooterTemplate
                };

                var result = await page.PdfDataAsync(puppeteerPdfOptions);

                _logger.LogInformation("Puppeteer PDF generation completed in {Elapsed}ms.", watcher.ElapsedMilliseconds);

                return result;
            }
            finally
            {
                if (page is { IsClosed: false })
                {
                    await page.CloseAsync();
                }

                await _pagePoolService.ReturnPageAsync(page);
            }
        });

        return result;
    }

    private IAsyncPolicy<byte[]> BuildRetryPolicy()
    {
        _ = int.TryParse(Environment.GetEnvironmentVariable("PUPPETEER_RETRY_COUNT"), out var retryCount)
                ? retryCount
                : 3;

        _ = int.TryParse(Environment.GetEnvironmentVariable("PUPPETEER_RETRY_INTERVAL"), out var retryInterval)
            ? retryInterval
            : 1000;

        var retryPolicy = Policy<byte[]>
            .Handle<Exception>()
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromMilliseconds(retryInterval * retryAttempt));

        var fallbackPolicy = Policy<byte[]>
            .Handle<Exception>()
            .FallbackAsync(
                [],
                (result, _) =>
                {
                    _logger.LogError(
                        result.Exception,
                        $"Puppeteer PDF generation failed after {retryCount} time retries.");

                    throw result.Exception;
                });


        return Policy.WrapAsync(retryPolicy, fallbackPolicy);
    }

    private PaperFormat GetPaperFormat(string format)
    {
        return format?.ToLower() switch
        {
            "letter" => PaperFormat.Letter,
            "legal" => PaperFormat.Legal,
            "tabloid" => PaperFormat.Tabloid,
            "ledger" => PaperFormat.Ledger,
            "a0" => PaperFormat.A0,
            "a1" => PaperFormat.A1,
            "a2" => PaperFormat.A2,
            "a3" => PaperFormat.A3,
            "a4" => PaperFormat.A4,
            "a5" => PaperFormat.A5,
            "a6" => PaperFormat.A6,
            _ => PaperFormat.A4 // default to A4 if format is not recognized
        };
    }
}

