using System.Diagnostics;
using Polly;
using PuppeteerPdfGenerator.Api.Models;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace PuppeteerPdfGenerator.Api.Services;

public class PdfGeneratorService : IPdfGeneratorService
{
    private static readonly int MaxConcurrentPages = int.TryParse(Environment.GetEnvironmentVariable("MaxConcurrentPages"), out var maxConcurrentPages)
        ? maxConcurrentPages
        : 10;

    private static readonly SemaphoreSlim Semaphore = new(MaxConcurrentPages, MaxConcurrentPages);

    private readonly IBrowser _browser;
    private readonly ILogger<PdfGeneratorService> _logger;

    private readonly IAsyncPolicy<byte[]> _retryPolicy;

    public PdfGeneratorService(IBrowser browser, ILogger<PdfGeneratorService> logger)
    {
        _browser = browser;
        _logger = logger;
        _retryPolicy = BuildRetryPolicy();
    }

    public async Task<byte[]> GeneratePdfAsync(GeneratePdfParams options, CancellationToken cancellationToken)
    {
        var watcher = new Stopwatch();

        var result = await _retryPolicy.ExecuteAsync(async () =>
        {
            await Semaphore.WaitAsync(cancellationToken);
            await using var page = await _browser.NewPageAsync();

            try
            {
                // Navigate to URL if provided
                if (!string.IsNullOrEmpty(options.Url))
                {
                    await page.GoToAsync(options.Url);
                }

                var puppeteerPdfOptions = new PuppeteerSharp.PdfOptions
                {
                    Format = GetPaperFormat(options.PdfOptions.Format),
                    PrintBackground = options.PdfOptions.PrintBackground,
                    DisplayHeaderFooter = options.PdfOptions.DisplayHeaderFooter,
                    MarginOptions = new PuppeteerSharp.Media.MarginOptions
                    {
                        Top = options.PdfOptions.Margin.Top,
                        Bottom = options.PdfOptions.Margin.Bottom
                    },
                    HeaderTemplate = options.PdfOptions.HeaderTemplate,
                    FooterTemplate = options.PdfOptions.FooterTemplate
                };

                return await page.PdfDataAsync(puppeteerPdfOptions);
            }
            finally
            {
                if (page is { IsClosed: false })
                {
                    await page.CloseAsync();
                }

                Semaphore.Release();
            }
        });

        return result;
    }

    private IAsyncPolicy<byte[]> BuildRetryPolicy()
    {
        _ = int.TryParse(Environment.GetEnvironmentVariable("PuppeteerRetryCount"), out var retryCount)
                ? retryCount
                : 3;

        _ = int.TryParse(Environment.GetEnvironmentVariable("PuppeteerRetryInterval"), out var retryInterval)
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

