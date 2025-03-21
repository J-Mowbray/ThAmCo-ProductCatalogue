using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalogue.Services.ProductsRepo;
using ProductCatalogue.Services.UnderCutters;

namespace ProductCatalogue.Controllers;

[ApiController]
[Route("[controller]")]
public class DebugController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IUnderCuttersService _underCuttersService;
    private readonly IProductsRepo _productsRepo;
    
    public DebugController(ILogger<DebugController> logger,
                                        IUnderCuttersService underCuttersService,
                                        IProductsRepo productsRepo)
    {
        _logger = logger;
        _underCuttersService = underCuttersService;
        _productsRepo = productsRepo;
    }

    // GET: /debug/undercutters
    [HttpGet("UnderCutters")]
    public async Task<IActionResult> UnderCutters()
    
    {
        IEnumerable<ProductDto> products = null!;
        try
        {
            products = await _underCuttersService.GetProductsAsync();
  
        }
        catch
        {
            _logger.LogWarning("Exception occured when using the UnderCutters service");
            products = Array.Empty<ProductDto>();
        }
        return Ok(products.ToList());
    }

        // GET: /debug/repo
    [HttpGet("repo")]
    public async Task<IActionResult> Repo()
    
    {
        IEnumerable<Product> products = null!;
        try
        {
            products = await _productsRepo.GetProductsAsync();
  
        }
        catch
        {
            _logger.LogWarning("Exception occured when using the Products repo");
            products = Array.Empty<Product>();
        }
        return Ok(products.ToList());
    }
}
