using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    // Add these two missing methods
    
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var dbProduct = await _productsContext.Products.FindAsync(id);
        
        if (dbProduct == null)
        {
            return null;
        }
        
        return new Product
        {
            Id = dbProduct.Id,
            Name = dbProduct.Name,
            Ean = dbProduct.Ean,
            CategoryId = dbProduct.CategoryId,
            CategoryName = dbProduct.CategoryName,
            BrandId = dbProduct.BrandId,
            BrandName = dbProduct.BrandName,
            Description = dbProduct.Description,
            Price = dbProduct.Price,
            InStock = dbProduct.InStock,
            ExpectedRestock = dbProduct.ExpectedRestock
        };
    }
    
    public async Task<IEnumerable<Product>> SearchProductsAsync(string? searchTerm, int? categoryId, int? brandId)
    {
        IQueryable<ProductCatalogue.Data.Products.Product> query = _productsContext.Products;
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTerm) || 
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
        }
        
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }
        
        if (brandId.HasValue)
        {
            query = query.Where(p => p.BrandId == brandId.Value);
        }
        
        var dbProducts = await query.ToListAsync();
        
        return dbProducts.Select(p => new Product
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
        });
    }
}