using System.Diagnostics;
using Polly;
using PuppeteerPdfGenerator.Api.Models;
using PuppeteerSharp;

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

    public async Task<byte[]> GeneratePdfAsync(GeneratePdfOptions options, CancellationToken cancellationToken)
    {
        var watcher = new Stopwatch();

        var result = await _retryPolicy.ExecuteAsync(async () =>
        {
            await Semaphore.WaitAsync(cancellationToken);
            await using var page = await _browser.NewPageAsync();

            try
            {
                await Semaphore.WaitAsync();

                await page.SetContentAsync(
                    options.ContentHtml,
                    new NavigationOptions
                    {
                        WaitUntil = new[] { WaitUntilNavigation.Networkidle0, WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded },
                    });

                var pdfBytes = await page.PdfDataAsync(options.PdfOptions);

                watcher.Reset();

                return await page.PdfDataAsync();
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

