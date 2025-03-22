using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProductCatalogue.Services.UnderCutters;

public class UnderCuttersService : IUnderCuttersService
{
    private readonly HttpClient _client;
    private readonly ILogger<UnderCuttersService> _logger;

    public UnderCuttersService(HttpClient client, ILogger<UnderCuttersService> logger)
    {
        _client = client;
        _logger = logger;
        
        
        _client.Timeout = TimeSpan.FromSeconds(30);
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching products from UnderCutters API");
            
            var uri = "api/product";
            var response = await _client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            
            var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
            return products ?? Array.Empty<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products from UnderCutters API");
            throw; 
        }
    }
    
    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(
        int? categoryId = null,
        string? categoryName = null,
        int? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (categoryId.HasValue)
                queryParams.Add($"category_id={categoryId}");
                
            if (!string.IsNullOrEmpty(categoryName))
                queryParams.Add($"category_name={Uri.EscapeDataString(categoryName)}");
                
            if (brandId.HasValue)
                queryParams.Add($"brand_id={brandId}");
                
            if (minPrice.HasValue)
                queryParams.Add($"min_price={minPrice}");
                
            if (maxPrice.HasValue)
                queryParams.Add($"max_price={maxPrice}");
            
            string requestUri = "api/product";
            if (queryParams.Any())
                requestUri += "?" + string.Join("&", queryParams);
            
            _logger.LogInformation($"Searching products from UnderCutters API: {requestUri}");
            var response = await _client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            
            var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
            return products ?? Array.Empty<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products from UnderCutters API");
            throw;
        }
    }
}