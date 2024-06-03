using Cw9.Data;
using Microsoft.AspNetCore.Mvc;

namespace Cw9.Controllers;
[ApiController]
[Route("/api/[controller]")]
public class ClientsController: ControllerBase
{
    private readonly Cw8Context _context;

    public ClientsController(Cw8Context context)
    {
        _context = context;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var find = await _context.Clients.FindAsync(idClient);
        if (find==null)
        {
            return NotFound($"Client with id={idClient} was not found");
        }

        var cts = _context.ClientTrips;
        var hasTrips = cts.Select(ct=>ct.IdClientNavigation).Where(c=>c==find).ToList().Any();
        if (hasTrips)
        {
            return BadRequest($"Client with id={idClient} has trips");
        }

        _context.Clients.Remove(find);
        return Ok();
    }
}