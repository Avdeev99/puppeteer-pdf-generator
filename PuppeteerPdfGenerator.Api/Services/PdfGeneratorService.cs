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
                    Format = (PaperFormat)Enum.Parse(typeof(PaperFormat), options.PdfOptions.Format, true),
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
}

