using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Application.Products.Commands;
using Ambev.DeveloperEvaluation.Application.Products.Queries;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Extensions;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(IMediator mediator, IConfiguration config, ICacheService cache) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginationResult<ProductDto>>> GetProducts(CancellationToken ct)
    {
        var defPage = config.GetSection("Pagination").GetValue<int>("DefaultPage", 1);
        var defSize = config.GetSection("Pagination").GetValue<int>("DefaultSize", 10);
        var maxSize = config.GetSection("Pagination").GetValue<int>("MaxSize", 100);

        var queryParams = QueryParsingExtensions.ParseFrom(Request, defPage, defSize, maxSize);

        var cacheKey = $"products:list:p={queryParams.Page}:s={queryParams.Size}:o={queryParams.Order}:{string.Join('&', queryParams.Equality.Select(k => $"{k.Key}={k.Value}"))}:{string.Join('&', queryParams.Wildcards.Select(k => $"{k.Key}={k.Value}"))}:{string.Join('&', queryParams.Ranges.Select(k => $"{k.Key}={k.Value.Min}-{k.Value.Max}"))}";

        var cached = await cache.GetAsync<PaginationResult<ProductDto>>(cacheKey, ct);
        if (cached is not null) return Ok(cached);

        var result = await mediator.Send(new GetProductsQuery(queryParams), ct);
        await cache.SetAsync(cacheKey, result, TimeSpan.FromSeconds(60), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(string id, CancellationToken ct)
    {
        var cacheKey = $"products:item:{id}";
        var cached = await cache.GetAsync<ProductDto>(cacheKey, ct);
        if (cached is not null) return Ok(cached);

        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        if (result is null) return NotFound(new { type = "ResourceNotFound", error = "Product not found", detail = $"The product with ID {id} does not exist" });

        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductDto dto, CancellationToken ct)
    {
        if (dto.Price < 0) return BadRequest(new { type = "ValidationError", error = "Invalid input data", detail = "The 'price' field must be a positive number" });

        var created = await mediator.Send(new CreateProductCommand(dto), ct);
        
        // Invalidate relevant caches
        await cache.RemoveByPrefixAsync("products:list:", ct);
        await cache.RemoveByPrefixAsync("products:categories", ct);
        
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> Update(string id, [FromBody] ProductDto dto, CancellationToken ct)
    {
        if (dto.Price < 0) return BadRequest(new { type = "ValidationError", error = "Invalid input data", detail = "The 'price' field must be a positive number" });

        var updated = await mediator.Send(new UpdateProductCommand(id, dto), ct);
        if (updated is null) return NotFound(new { type = "ResourceNotFound", error = "Product not found", detail = $"The product with ID {id} does not exist" });

        // Invalidate relevant caches
        await cache.RemoveByPrefixAsync("products:list:", ct);
        await cache.RemoveByPrefixAsync("products:item:", ct);
        await cache.RemoveByPrefixAsync("products:categories", ct);
        
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id), ct);
        
        // Invalidate relevant caches
        await cache.RemoveByPrefixAsync("products:list:", ct);
        await cache.RemoveByPrefixAsync("products:item:", ct);
        await cache.RemoveByPrefixAsync("products:categories", ct);
        
        return Ok(new { message = "Product deleted successfully" });
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> Categories(CancellationToken ct)
    {
        var cacheKey = "products:categories";
        var cached = await cache.GetAsync<IEnumerable<string>>(cacheKey, ct);
        if (cached is not null) return Ok(cached);

        var cats = await mediator.Send(new GetCategoriesQuery(), ct);
        await cache.SetAsync(cacheKey, cats, TimeSpan.FromMinutes(2), ct);
        return Ok(cats);
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<PaginationResult<ProductDto>>> ByCategory(string category, CancellationToken ct)
    {
        var defPage = config.GetSection("Pagination").GetValue<int>("DefaultPage", 1);
        var defSize = config.GetSection("Pagination").GetValue<int>("DefaultSize", 10);
        var maxSize = config.GetSection("Pagination").GetValue<int>("MaxSize", 100);

        var queryParams = QueryParsingExtensions.ParseFrom(Request, defPage, defSize, maxSize);

        var cacheKey = $"products:list:cat={category}:p={queryParams.Page}:s={queryParams.Size}:o={queryParams.Order}";
        var cached = await cache.GetAsync<PaginationResult<ProductDto>>(cacheKey, ct);
        if (cached is not null) return Ok(cached);

        var result = await mediator.Send(new GetProductsByCategoryQuery(category, queryParams), ct);
        await cache.SetAsync(cacheKey, result, TimeSpan.FromSeconds(60), ct);
        return Ok(result);
    }
}
