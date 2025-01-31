using System.Threading.Channels;
using PuppeteerSharp;

namespace PuppeteerPdfGenerator.Api.Services;

public class PagePoolService : IPagePoolService
{
    private static readonly int MaxConcurrentPages = int.TryParse(Environment.GetEnvironmentVariable("PUPPETEER_MAX_CONCURRENT_PAGES"), out var maxConcurrentPages)
        ? maxConcurrentPages
        : 15;

    private readonly Channel<IPage> _pagePool;
    private readonly List<IPage> _allPages = new();
    private readonly IBrowser _browser;
    private readonly ILogger<PagePoolService> _logger;

    public PagePoolService(IBrowser browser, ILogger<PagePoolService> logger)
    {
        _browser = browser;
        _logger = logger;
        _pagePool = Channel.CreateBounded<IPage>(new BoundedChannelOptions(MaxConcurrentPages)
        {
            FullMode = BoundedChannelFullMode.Wait, 
        });
        
        InitializePagePool().GetAwaiter().GetResult();
    }

    private async Task InitializePagePool()
    {
        for (int i = 0; i < MaxConcurrentPages; i++)
        {
            var page = await _browser.NewPageAsync();
            _allPages.Add(page);
            await _pagePool.Writer.WriteAsync(page);
        }
        _logger.LogInformation($"Initialized page pool with {MaxConcurrentPages} pages");
    }

    public async Task<IPage> GetPageAsync(CancellationToken cancellationToken = default)
    {
        await _pagePool.Reader.WaitToReadAsync();
        var page = await _pagePool.Reader.ReadAsync(cancellationToken);

        _logger.LogInformation("Got page from pool.");

        return page;
    }

    public async Task ReturnPageAsync(IPage page, CancellationToken cancellationToken = default)
    {
        page = await RepairPageAsync(page);
        await _pagePool.Writer.WriteAsync(page, cancellationToken);

        _logger.LogInformation("Returned page to pool.");
    }

    private async Task<IPage> RepairPageAsync(IPage page)
    {
        if (page.IsClosed || page.Client == null || page.Client.CloseReason != null)
        {
            _logger.LogWarning("Recreating page during return to pool.");
            page = await _browser.NewPageAsync();
        }
        else
        {
            await page.SetContentAsync("<html><body></body></html>");
        }

        return page;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var page in _allPages)
        {
            if (!page.IsClosed)
            {
                await page.CloseAsync();
            }
        }

        _logger.LogInformation("All pages from pool disposed.");
    }
}