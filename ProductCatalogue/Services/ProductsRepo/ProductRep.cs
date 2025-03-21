using System;
using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Data.Products;

namespace ProductCatalogue.Services.ProductsRepo;

public class ProductsRepo : IProductsRepo
{
    private readonly ProductsContext _productsContext;

    public ProductsRepo(ProductsContext productsContext)
    {
        _productsContext = productsContext;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        var products = await _productsContext.Products.Select(p => new Product
        {
            Id = p.Id,
            Name = p.Name,
            Ean = p.Ean,
            CategoryId = p.CategoryId,
            CategoryName = p.CategoryName,
            BrandId = p.BrandId,
            BrandName = p.BrandName,
            Description = p.Description,
            Price = p.Price,
            InStock = p.InStock,
            ExpectedRestock = p.ExpectedRestock

        }).ToListAsync();
        return products; 
    }
}