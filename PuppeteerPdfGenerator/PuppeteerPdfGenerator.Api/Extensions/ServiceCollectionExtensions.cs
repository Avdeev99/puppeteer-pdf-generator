using PuppeteerSharp;

namespace PuppeteerPdfGenerator.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
        services.RegisterBrowser();

        return services;
    }

    private static IServiceCollection RegisterBrowser(this IServiceCollection services)
    {
        PuppeteerSharp.Helpers.TaskHelper.DefaultTimeout = int.TryParse(Environment.GetEnvironmentVariable("EStatementProcessor__PdfSettings__PuppeteerDefaultTimeout"), out var defaultTimeout)
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

        _ = int.TryParse(Environment.GetEnvironmentVariable("EStatementProcessor__PdfSettings__PuppeteerProtocolTimeout"), out var protocolTimeout)
            ? protocolTimeout
            : 30000;

        var options = new LaunchOptions
        {
            Headless = true,
            ExecutablePath = Environment.GetEnvironmentVariable("CHROME_EXECUTABLE_PATH"),
            Args = launchArgs,
            ProtocolTimeout = protocolTimeout,
        };

        var browser = Puppeteer.LaunchAsync(options).Result;

        services.AddSingleton(browser);

        return services;
    }
}
