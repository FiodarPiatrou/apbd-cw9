using Cw9.Data;
using Cw9.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cw9.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly Cw8Context _context;

    public TripsController(Cw8Context context)
    {
        _context = context;
    }
    [HttpGet]
    public async  Task<IActionResult> GetTrips([FromQuery] int pageNum=1,[FromQuery] int pageSize=10)
    {
        var trips = await _context.Trips
            .Select(t=>new
            {
                t.Name,
                t.Description,
                t.DateFrom,
                t.DateTo,
                t.MaxPeople,
                Clients=t.ClientTrips.Select(ct=>new{ct.IdClientNavigation.FirstName,ct.IdClientNavigation.LastName}),
                Countries=t.IdCountries.Select(c=>new {c.Name})
                
            })
            .OrderByDescending(t => t.DateFrom)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Trips.CountAsync();

        var response = new
        {
            PageNumber = pageNum,
            PageSize = pageSize,
            AllPages = totalCount,
            Trips = trips
        };

        return Ok(response);
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> PostClient(int idTrip, [FromBody] ClientDTO clientDto)
    {
        var find = _context.Clients.First(c => c.Pesel == clientDto.Pesel);
        if (find!=null)
        {
            return BadRequest($"Client with pesel={find.Pesel} already exists");
        }
        var isSignedUp=
    }
}