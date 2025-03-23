// ProductCatalogue/Services/ProductsRepo/ProductsRepoFake.cs (note the "s" in Products)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogue.Services.ProductsRepo;

public class ProductsRepoFake : IProductsRepo
{
    private readonly Product[] _products =
    {
        new Product { Id = 1, Ean = "5901234123457", CategoryId = 3, CategoryName = "Electronics", BrandId = 7, BrandName = "Tech Essential", Name = "Premium Wireless Headphones", Description = "High-quality wireless headphones with noise cancellation.", Price = 129.99m, InStock = true, ExpectedRestock = null },
        new Product { Id = 2, Ean = "8712345678906", CategoryId = 5, CategoryName = "Kitchen", BrandId = 12, BrandName = "Italian Express", Name = "Pizza Oven", Description = "Get that fine taste of Italy with your very own Pizza Oven!", Price = 89.95m, InStock = true, ExpectedRestock = null },
        new Product { Id = 3, Ean = "4006381333931", CategoryId = 2, CategoryName = "Books", BrandId = 9, BrandName = "Red Ribbon", Name = "The Midnight Getaway", Description = "Bestselling crime novel based on the lives of Bonnie & Clyde.", Price = 12.50m, InStock = false, ExpectedRestock = new DateTime(2023, 5, 15) },
        new Product { Id = 4, Ean = "7350053850025", CategoryId = 7, CategoryName = "Sports", BrandId = 3, BrandName = "Active Fit", Name = "Premium Yoga Mat", Description = "Eco-friendly non-slip yoga mat with alignment markings. Get fit, with Active Fit!", Price = 45.00m, InStock = true, ExpectedRestock = null },
        new Product { Id = 5, Ean = "4891945901234", CategoryId = 1, CategoryName = "Fashion", BrandId = 21, BrandName = "Urban", Name = "Denim Jacket", Description = "Rock this denim jacket like it's 1982 again! Made with premium denim.", Price = 79.99m, InStock = true, ExpectedRestock = null },
        new Product { Id = 6, Ean = "7622210123459", CategoryId = 4, CategoryName = "Food", BrandId = 15, BrandName = "RadDury's", Name = "Chocolate Orange", Description = "The best chocolate orange you'll ever have!", Price = 3.79m, InStock = false, ExpectedRestock = new DateTime(2023, 3, 28) }
    };

    public Task<IEnumerable<Product>> GetProductsAsync()
    {
        return Task.FromResult(_products.AsEnumerable());
    }

    public Task<Product?> GetProductByIdAsync(int id)
    {
        return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
    }

    public Task<IEnumerable<Product>> SearchProductsAsync(string? searchTerm, int? categoryId, int? brandId)
    {
        var query = _products.AsEnumerable();
        
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
        
        return Task.FromResult(query);
    }
}