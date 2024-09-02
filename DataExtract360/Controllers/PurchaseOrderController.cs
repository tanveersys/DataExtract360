using DataExtract360.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace DataExtract360.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly PdfDataExtractor _pdfDataExtractor;

        public PurchaseOrderController(PdfDataExtractor pdfDataExtractor)
        {
            _pdfDataExtractor = pdfDataExtractor;
        }

        [HttpPost("extract")]
        public async Task<IActionResult> ExtractData(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var filePath = Path.GetTempFileName();
                await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());

                var purchaseOrder = _pdfDataExtractor.ExtractData(filePath);
                System.IO.File.Delete(filePath);

                return Ok(purchaseOrder);
            }
        }
    }
}
