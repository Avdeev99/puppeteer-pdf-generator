using Microsoft.AspNetCore.Mvc;
using PuppeteerPdfGenerator.Api.Models;
using PuppeteerPdfGenerator.Api.Services;

namespace PuppeteerPdfGenerator.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class PdfGeneratorController : ControllerBase
    {
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly ILogger<PdfGeneratorController> _logger;

        public PdfGeneratorController(
            IPdfGeneratorService pdfGeneratorService,
            ILogger<PdfGeneratorController> logger)
        {
            _pdfGeneratorService = pdfGeneratorService;
            _logger = logger;
        }

        [HttpPost("rpc")]
        public async Task<IActionResult> HandleRpcRequest([FromBody] JsonRpcRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Method != "generatePdf")
                {
                    return Ok(new JsonRpcErrorResponse
                    {
                        Error = new JsonRpcError
                        {
                            Code = -32601,
                            Message = $"Method {request.Method} not found"
                        },
                        Id = request.Id
                    });
                }

                _logger.LogInformation("Generating PDF");

                var pdfBytes = await _pdfGeneratorService.GeneratePdfAsync(request.Params, cancellationToken);
                var base64Pdf = Convert.ToBase64String(pdfBytes);

                return Ok(new JsonRpcSuccessResponse
                {
                    Result = base64Pdf,
                    Id = request.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF");
                
                return StatusCode(500, new JsonRpcErrorResponse
                {
                    Error = new JsonRpcError
                    {
                        Code = -32603,
                        Message = ex.Message
                    },
                    Id = request.Id
                });
            }
        }
    }
}
