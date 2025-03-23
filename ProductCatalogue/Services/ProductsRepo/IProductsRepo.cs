// ProductCatalogue/Services/ProductsRepo/IProductsRepo.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductCatalogue.Services.ProductsRepo;

public interface IProductsRepo
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<IEnumerable<Product>> SearchProductsAsync(string? searchTerm, int? categoryId, int? brandId);
}