using Microsoft.AspNetCore.Mvc;
using Orders.Application;

namespace Orders.Api;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IOrderCreator creator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<OrderCreatedResult>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await creator.CreateAsync(new CreateOrderCommand(request.Description), cancellationToken);
            return Created($"/api/orders/{result.Id}", result);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public sealed record CreateOrderRequest(string Description);
