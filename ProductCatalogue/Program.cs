using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductCatalogue.Data.Products;
using ProductCatalogue.Services.UnderCutters;
using System.Security.Claims;
using Polly;
using Polly.Extensions.Http;
using ProductCatalogue.Services.ProductsRepo;
using Microsoft.VisualBasic;
using ProductCatalogue.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add authentication and authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });
builder.Services.AddAuthorization();

// Register UnderCuttersService with appropriate implementation for environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IUnderCuttersService, UnderCuttersServiceFake>();
}
else
{
    // Production - use real service with Polly for resilience
    builder.Services.AddHttpClient<IUnderCuttersService, UnderCuttersService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["WebServices:UnderCutters:BaseUrl"]);
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
}

// Configure DB context
builder.Services.AddDbContext<ProductsContext>(options =>
{       /* Temporary uncomment to generate SQL for Azure
        options.UseSqlServer("Server=dummyserver;Database=dummydb;User Id=dummy;Password=dummy;");
        */
    
    // Temporary comment this section out when generating SQL for Azure
    if (builder.Environment.IsDevelopment())
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = System.IO.Path.Join(path, "products.db");
        options.UseSqlite($"Data Source={dbPath}");
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
    else
    {
        var cs = builder.Configuration.GetConnectionString("ProductsContext");
        options.UseSqlServer(cs, sqlServerOptionsAction: sqlOptions =>
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(3),
                errorNumbersToAdd: null
            )
        );
    } // Comment this out when using SQL for Azure
    
});

// Register ProductsRepo
builder.Services.AddTransient<IProductsRepo, ProductsRepo>();
builder.Services.AddHostedService<ProductSyncService>();

var app = builder.Build();

// Seed development data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var env = services.GetRequiredService<IWebHostEnvironment>();
    
    if (env.IsDevelopment())
    {
        var context = services.GetRequiredService<ProductsContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            // Use EnsureCreated to build schema from model in development
            // This bypasses migrations completely for local SQLite
            logger.LogInformation("Creating database from model...");
            context.Database.EnsureDeleted(); // Optional: for clean slate
            context.Database.EnsureCreated();
            
            // Check if seeding is needed
            if (!context.Products.Any())
            {
                logger.LogInformation("Seeding test data...");
                await ProductsInitaliser.SeedTestData(context);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating or seeding database");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication middleware before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Helper methods for Polly policies
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(
            6, // Number of retry attempts
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                // You can log retry attempts here if needed
                Console.WriteLine($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            5, // Number of exceptions or failures before breaking the circuit
            TimeSpan.FromSeconds(30) // Duration circuit opens before retry
        );
}