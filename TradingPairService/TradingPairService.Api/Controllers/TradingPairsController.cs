using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingPairService.Application.Dto;
using TradingPairService.Application.Interfaces.Services;

namespace TradingPairService.Api.Controllers;

/// <summary>
/// The pair catalog is public to read (the SPA lists pairs, other services resolve them), but
/// mutating it changes what the whole exchange can trade, so writes require an authenticated user.
/// </summary>
[ApiController]
[Route("api/trading-pairs")]
public class TradingPairsController : ControllerBase
{
    private readonly ITradingPairService _service;

    public TradingPairsController(ITradingPairService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    [HttpGet("{symbol}")]
    public async Task<IActionResult> GetBySymbol(string symbol)
    {
        var pair = await _service.GetBySymbol(symbol);
        return pair is null ? NotFound() : Ok(pair);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateTradingPairRequest request)
    {
        var pair = await _service.Create(request);
        return CreatedAtAction(nameof(GetBySymbol), new { symbol = pair.Symbol }, pair);
    }

    [Authorize]
    [HttpPatch("{symbol}/activate")]
    public async Task<IActionResult> Activate(string symbol)
    {
        await _service.Activate(symbol);
        return NoContent();
    }

    [Authorize]
    [HttpPatch("{symbol}/deactivate")]
    public async Task<IActionResult> Deactivate(string symbol)
    {
        await _service.Deactivate(symbol);
        return NoContent();
    }
}