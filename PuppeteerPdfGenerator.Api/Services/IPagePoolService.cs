using PuppeteerSharp;

namespace PuppeteerPdfGenerator.Api.Services;

public interface IPagePoolService : IAsyncDisposable
{
    Task<IPage> GetPageAsync(CancellationToken cancellationToken = default);

    Task ReturnPageAsync(IPage page, CancellationToken cancellationToken = default);
}