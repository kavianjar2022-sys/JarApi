using JarApi.Data;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CompanyController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetAll()
    {
        var items = await _context.Companies.ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> Get(int id)
    {
        var item = await _context.Companies.FindAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Company>> Create(Company model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _context.Companies.Add(model);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, Company model)
    {
        if (id != model.Id) return BadRequest();

        var exists = await _context.Companies.AnyAsync(c => c.Id == id);
        if (!exists) return NotFound();

        _context.Entry(model).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var item = await _context.Companies.FindAsync(id);
        if (item == null) return NotFound();

        _context.Companies.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
