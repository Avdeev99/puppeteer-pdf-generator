using PuppeteerSharp;
using PuppeteerPdfGenerator.Api.Services;
namespace PuppeteerPdfGenerator.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IPagePoolService, PagePoolService>();
        services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
        services.RegisterBrowser();

        return services;
    }

    private static IServiceCollection RegisterBrowser(this IServiceCollection services)
    {
        PuppeteerSharp.Helpers.TaskHelper.DefaultTimeout = int.TryParse(Environment.GetEnvironmentVariable("PUPPETEER_DEFAULT_TIMEOUT"), out var defaultTimeout)
            ? defaultTimeout
            : 30000;

        var launchArgs = new[]
        {
            "--font-render-hinting=none",
            "--disable-blink-features=LayoutNGPrinting",
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
        };

        _ = int.TryParse(Environment.GetEnvironmentVariable("PUPPETEER_PROTOCOL_TIMEOUT"), out var protocolTimeout)
            ? protocolTimeout
            : 30000;

        var chromiumPath = Environment.GetEnvironmentVariable("CHROMIUM_PATH") ?? "/usr/bin/chromium";

        var options = new LaunchOptions
        {
            Headless = true,
            ExecutablePath = chromiumPath,
            Args = launchArgs,
            ProtocolTimeout = protocolTimeout,
        };

        var browser = Puppeteer.LaunchAsync(options).Result;

        services.AddSingleton(browser);

        return services;
    }
}
