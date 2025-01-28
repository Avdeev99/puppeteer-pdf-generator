using PuppeteerPdfGenerator.Api.Models;

namespace PuppeteerPdfGenerator.Api.Services;

public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePdfAsync(GeneratePdfParams options, CancellationToken cancellationToken);
}