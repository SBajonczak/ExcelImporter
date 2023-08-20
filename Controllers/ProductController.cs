using Microsoft.AspNetCore.Mvc;

namespace ExcelImporter.WebApi.Controllers;

/// <summary>
/// Controller to interact with the product data.
/// </summary>
[ApiController]
[Route("products")]
public class ProductController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }


    /// <summary>
    /// Uploadendpoint to get the desired exxcel file.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("upload")]
    [DisableRequestSizeLimit] // Allow any size (only for demo purpose)
    public async Task<ActionResult> Upload(CancellationToken ct)
    {
        if (Request.Form.Files.Count == 0) return NoContent();

        var file = Request.Form.Files[0];
        var filePath = SaveFile(file);
        
        // load product requests from excel file
        var productRequests = ExcelHelper.Import<ProductRequest>(filePath);
        var products = new List<Product>();
        Object lockMe = new Object();  
        // save product requests to database
        Parallel.ForEach(productRequests, productRequest =>
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = productRequest.Name,
                Quantity = productRequest.Quantity,
                Price = productRequest.Price,
                IsActive = productRequest.IsActive,
                ExpiryDate = productRequest.ExpiryDate
            };
            lock (lockMe)
            {
                products.Add(product);
            }
        });

        await _context.AddRangeAsync(products, ct);
        await _context.SaveChangesAsync(ct);

        return Ok();
    }

    /// <summary>
    /// Save the file into the root folder.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private string SaveFile(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new BadHttpRequestException("File is empty.");
        }

        var extension = Path.GetExtension(file.FileName);

        var webRootPath = _webHostEnvironment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }
            
        var folderPath = Path.Combine(webRootPath, "uploads");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
            
        var fileName = $"{Guid.NewGuid()}.{extension}";
        var filePath = Path.Combine(folderPath, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        file.CopyTo(stream);

        return filePath;
    }
}