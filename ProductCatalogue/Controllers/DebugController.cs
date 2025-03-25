using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCatalogue.Data.Products;
using ProductCatalogue.Services.ProductsRepo;
using ProductCatalogue.Services.UnderCutters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogue.Controllers;

[ApiController]
[Route("[controller]")]
public class DebugController : ControllerBase
{
    private readonly ILogger<DebugController> _logger;
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
        IEnumerable<ProductCatalogue.Services.ProductsRepo.Product> products = null!;
        try
        {
            products = await _productsRepo.GetProductsAsync();

        }
        catch
        {
            _logger.LogWarning("Exception occured when using the Products repo");
            products = Array.Empty<ProductCatalogue.Services.ProductsRepo.Product>();
        }
        return Ok(products.ToList());
    }

    [HttpPost("sync-all-products")]
    public async Task<IActionResult> SyncAllProducts()
    {
        _logger.LogInformation("Manual sync of ALL products requested");

        try
        {
            // Create a new scope to resolve services
            using var scope = HttpContext.RequestServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductsContext>();
            var underCuttersService = scope.ServiceProvider.GetRequiredService<IUnderCuttersService>();

            // Test database connection first
            bool canConnect = await dbContext.Database.CanConnectAsync();

            // Get products from UnderCutters API
            var supplierProducts = await underCuttersService.GetProductsAsync();
            _logger.LogInformation("Retrieved {Count} products from supplier", supplierProducts.Count());

            // Get existing products from database
            var existingProducts = await dbContext.Products.ToListAsync();
            var existingProductsById = existingProducts.ToDictionary(p => p.Id);

            int added = 0, updated = 0;
            var errors = new List<string>();

            // Process ALL products instead of just 2
            foreach (var supplierProduct in supplierProducts)
            {
                try
                {
                    // Apply 10% markup as per requirements
                    decimal finalPrice = Math.Round(supplierProduct.Price * 1.10m, 2);

                    if (existingProductsById.TryGetValue(supplierProduct.Id, out var existingProduct))
                    {
                        // Update existing product
                        existingProduct.Ean = (supplierProduct.Ean ?? "").Replace(" ", "").Trim();
                        existingProduct.Name = supplierProduct.Name ?? "Unknown";
                        existingProduct.Description = supplierProduct.Description ?? "";
                        existingProduct.CategoryId = supplierProduct.CategoryId;
                        existingProduct.CategoryName = supplierProduct.CategoryName ?? "";
                        existingProduct.BrandId = supplierProduct.BrandId;
                        existingProduct.BrandName = supplierProduct.BrandName ?? "";
                        existingProduct.Price = finalPrice;
                        existingProduct.InStock = supplierProduct.InStock;
                        existingProduct.ExpectedRestock = supplierProduct.ExpectedRestock?.ToUniversalTime();

                        dbContext.Products.Update(existingProduct);
                        updated++;
                    }
                    else
                    {
                        // Add new product with cleaned data
                        var newProduct = new ProductCatalogue.Data.Products.Product
                        {
                            Id = supplierProduct.Id,
                            Ean = (supplierProduct.Ean ?? "").Replace(" ", "").Trim(),
                            Name = supplierProduct.Name ?? "Unknown Product",
                            Description = supplierProduct.Description ?? "",
                            CategoryId = supplierProduct.CategoryId,
                            CategoryName = supplierProduct.CategoryName ?? "",
                            BrandId = supplierProduct.BrandId,
                            BrandName = supplierProduct.BrandName ?? "",
                            Price = finalPrice,
                            InStock = supplierProduct.InStock,
                            ExpectedRestock = supplierProduct.ExpectedRestock?.ToUniversalTime()
                        };

                        dbContext.Products.Add(newProduct);
                        added++;
                    }

                    // Save changes after each product
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error saving product {supplierProduct.Id}: {ex.Message}");
                        _logger.LogError(ex, "Error saving product {Id}", supplierProduct.Id);
                        dbContext.ChangeTracker.Clear();
                    }
                }
                catch (Exception pex)
                {
                    errors.Add($"Error processing product {supplierProduct.Id}: {pex.Message}");
                    _logger.LogError(pex, "Error processing product {Id}", supplierProduct.Id);
                }
            }

            // Get final count
            int finalCount = await dbContext.Products.CountAsync();

            return Ok(new
            {
                success = true,
                message = $"Processed all {supplierProducts.Count()} products",
                stats = new
                {
                    supplierProductCount = supplierProducts.Count(),
                    initialProductCount = existingProducts.Count,
                    added,
                    updated,
                    finalProductCount = finalCount
                },
                errors = errors
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    // If for some reason the table gets broken, likely because of EF migration and not using SQL migration, call this 
    [HttpPost("sql-fix")]
    public async Task<IActionResult> CreateProductTable()
    {
        try
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductsContext>();

            // Drop and recreate the Products table with MORE GENEROUS field sizes
            await dbContext.Database.ExecuteSqlRawAsync(@"
            IF OBJECT_ID('Products', 'U') IS NOT NULL
            DROP TABLE Products;
            
            CREATE TABLE Products(
                Id INT NOT NULL PRIMARY KEY,
                Ean NVARCHAR(50) NULL,          -- Increased from 13 to 50
                CategoryId INT NOT NULL,
                CategoryName NVARCHAR(100) NULL, -- Increased from 50 to 100
                BrandId INT NOT NULL,
                BrandName NVARCHAR(100) NULL,    -- Increased from 50 to 100
                Name NVARCHAR(250) NOT NULL,     -- Increased from 200 to 250
                Description NVARCHAR(MAX) NULL,
                Price DECIMAL(18,2) NOT NULL,
                InStock BIT NOT NULL,
                ExpectedRestock DATETIME2 NULL
            );
            
            CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
            CREATE INDEX IX_Products_BrandId ON Products(BrandId);
        ");

            return Ok(new
            {
                success = true,
                message = "Products table recreated with correct SQL Server types and increased field sizes"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    // Check the health of the database
    [HttpGet("health/database")]
    public async Task<IActionResult> DatabaseHealthCheck()
    {
        try
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductsContext>();

            bool canConnect = await dbContext.Database.CanConnectAsync();
            int productCount = await dbContext.Products.CountAsync();

            return Ok(new
            {
                status = canConnect ? "healthy" : "unhealthy",
                products = productCount,
                provider = dbContext.Database.ProviderName
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "unhealthy",
                error = ex.Message
            });
        }
    }
}

