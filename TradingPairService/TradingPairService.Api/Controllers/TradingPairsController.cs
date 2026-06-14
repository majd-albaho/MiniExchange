using Microsoft.AspNetCore.Mvc;
using TradingPairService.Application.Dto;
using TradingPairService.Application.Interfaces.Services;

namespace TradingPairService.Api.Controllers;

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

    [HttpPost]
    public async Task<IActionResult> Create(CreateTradingPairRequest request)
    {
        var pair = await _service.Create(request);
        return CreatedAtAction(nameof(GetBySymbol), new { symbol = pair.Symbol }, pair);
    }

    [HttpPatch("{symbol}/activate")]
    public async Task<IActionResult> Activate(string symbol)
    {
        await _service.Activate(symbol);
        return NoContent();
    }

    [HttpPatch("{symbol}/deactivate")]
    public async Task<IActionResult> Deactivate(string symbol)
    {
        await _service.Deactivate(symbol);
        return NoContent();
    }
}