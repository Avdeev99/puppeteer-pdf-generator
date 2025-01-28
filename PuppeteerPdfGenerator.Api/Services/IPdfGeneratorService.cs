using PuppeteerPdfGenerator.Api.Models;

namespace PuppeteerPdfGenerator.Api.Services;

public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePdfAsync(GeneratePdfOptions options, CancellationToken cancellationToken);
}