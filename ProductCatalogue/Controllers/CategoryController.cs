using Microsoft.AspNetCore.Mvc;
using ProductCatalogue.Services.ProductsRepo;

namespace ProductCatalogue.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IProductsRepo _productsRepo;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(IProductsRepo productsRepo, ILogger<CategoryController> logger)
    {
        _productsRepo = productsRepo;
        _logger = logger;
    }

    // GET: api/Category
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            // Extract unique categories from products
            var products = await _productsRepo.GetProductsAsync();
            var categories = products
                .Select(p => new { p.CategoryId, p.CategoryName })
                .GroupBy(c => c.CategoryId)
                .Select(g => g.First())
                .ToList();
                
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    // GET: api/Category/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        try
        {
            // Find products in this category
            var products = await _productsRepo.SearchProductsAsync(null, id, null);
            
            if (!products.Any())
            {
                return NotFound();
            }
            
            // Extract category info from first matching product
            var firstProduct = products.First();
            var category = new { 
                firstProduct.CategoryId, 
                firstProduct.CategoryName,
                ProductCount = products.Count()
            };
                
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the category");
        }
    }
}