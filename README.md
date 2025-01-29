# Puppeteer PDF Generator

A C# JSON-RPC server that converts HTML content into PDF files using Puppeteer Sharp.

## Features

- HTML to PDF conversion
- Configurable PDF options (format, margins, headers, footers)
- JSON-RPC 2.0 API
- Docker support
- Page pooling for better performance
- Health check endpoints

## Prerequisites

- Docker
- .NET 8.0 SDK (for local development)

## Quick Start

### Using Docker

```bash
# Clone the repository
git clone https://github.com/yourusername/puppeteer-pdf-generator.git
cd puppeteer-pdf-generator

# Start the application
./docker-compose-up.sh
```

### Local Development

```bash
# Run the application
dotnet run --project PuppeteerPdfGenerator.Api
```

## API Endpoints

### Health Check

```bash
curl http://localhost:3000/api/health
```

### Ping

```bash
curl http://localhost:3000/api/ping
```

### Generate PDF

```bash
curl -X POST http://localhost:3000/api/rpc \
-H "Content-Type: application/json" \
-d '{
    "jsonrpc": "2.0",
    "method": "generatePdf",
    "params": {
        "contentHtml": "<html><body><h1>Hello World</h1></body></html>",
        "pdfOptions": {
            "format": "A4",
            "printBackground": true,
            "displayHeaderFooter": true,
            "margin": {
                "top": "1cm",
                "bottom": "1cm"
            },
            "headerTemplate": "<div style=\"font-size: 10px; text-align: center; width: 100%;\">Header</div>",
            "footerTemplate": "<div style=\"font-size: 10px; text-align: center; width: 100%;\">Footer</div>"
        }
    },
    "id": "1"
}'
```

## Configuration

Environment variables can be configured in docker-compose.yml:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - PUPPETEER_DEFAULT_TIMEOUT=60000
  - PUPPETEER_PROTOCOL_TIMEOUT=60000
  - PUPPETEER_MAX_CONCURRENT_PAGES=15
  - PUPPETEER_RETRY_COUNT=3
  - PUPPETEER_RETRY_INTERVAL=1000
```

## PDF Options

| Option | Type | Description |
|--------|------|-------------|
| format | string | Paper format (A4, Letter, etc.) |
| printBackground | boolean | Whether to print background graphics |
| displayHeaderFooter | boolean | Whether to display header and footer |
| margin | object | Page margins (top, bottom) |
| headerTemplate | string | HTML template for the print header |
| footerTemplate | string | HTML template for the print footer |

Supported paper formats:
- A0-A6
- Letter
- Legal
- Tabloid
- Ledger

## Development

### Debugging in Docker

1. Start the container with debugging enabled:
```bash
docker compose up --build
```

2. Attach your debugger (VS Code or Visual Studio) to the running container

### Running Tests

```bash
dotnet test
```

## License

[Your License]

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request
