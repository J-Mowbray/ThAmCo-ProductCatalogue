using Microsoft.AspNetCore.Mvc;
using ProductCatalogue.Services.ProductsRepo;

namespace ProductCatalogue.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase  // Note: Singular "Product" to match UnderCutters
{
    private readonly IProductsRepo _productsRepo;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductsRepo productsRepo, ILogger<ProductController> logger)
    {
        _productsRepo = productsRepo;
        _logger = logger;
    }

    // GET: api/Product?category_id={id}&category_name={name}&brand_id={id}&min_price={min}&max_price={max}
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery(Name = "category_id")] int? category_id = null,
        [FromQuery(Name = "category_name")] string? category_name = null,
        [FromQuery(Name = "brand_id")] int? brand_id = null,
        [FromQuery(Name = "min_price")] decimal? min_price = null,
        [FromQuery(Name = "max_price")] decimal? max_price = null)
    {
        try
        {
            // Convert the parameters to use your existing repository method
            // You might need to adapt your repo to handle all these parameters
            string? searchTerm = category_name; // Using category_name as search term for now
            
            var products = await _productsRepo.SearchProductsAsync(searchTerm, category_id, brand_id);
            
            // Apply price filtering if needed (if not handled by repository)
            if (min_price.HasValue || max_price.HasValue)
            {
                products = products.Where(p => 
                    (!min_price.HasValue || p.Price >= min_price.Value) &&
                    (!max_price.HasValue || p.Price <= max_price.Value));
            }
            
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    // GET: api/Product/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            var product = await _productsRepo.GetProductByIdAsync(id);
            
            if (product == null)
            {
                return NotFound();
            }
            
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }
}