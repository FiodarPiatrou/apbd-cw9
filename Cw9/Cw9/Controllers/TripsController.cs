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

        var trip = await _context.Trips.FindAsync(idTrip);
        var isSignedUp = trip.ClientTrips.Select(ct => ct.IdClientNavigation)
            .Where(c => clientDto.Pesel == c.Pesel).ToList().Any();
        if (isSignedUp)
        {
            return BadRequest($"client with pesel={clientDto.Pesel} already sighed up");
            
        }

        var isFromFuture = trip.DateFrom > DateTime.Now;
        if (!isFromFuture)
        {
            return BadRequest("DateFrom is in the past");
        }

        var idC = _context.Clients.Count() + 1;
        _context.Clients.Add(new Client
        {
            IdClient = idC,
            Email = clientDto.Email,
            FirstName = clientDto.FirstName,
            LastName = clientDto.LastName,
            Pesel = clientDto.Pesel,
            Telephone = clientDto.Telephone,


        });
        _context.ClientTrips.Add(new ClientTrip { IdClient = idC ,PaymentDate = clientDto.PaymentDate,
            RegisteredAt = DateTime.Now, IdTrip = idTrip});
        return Created();
    }
}