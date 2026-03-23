using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.DTOs;

namespace SportsStore.OrderAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        OrderDbContext context,
        IMapper mapper,
        ILogger<ProductsController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        _logger.LogInformation("GetAll products endpoint called");

        var products = await _context.Products.ToListAsync();
        var productDtos = _mapper.Map<List<ProductDto>>(products);
        return Ok(productDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        _logger.LogInformation("GetById product endpoint called - ProductId: {ProductId}", id);

        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        var productDto = _mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }
}
