using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using practice.Data;
using practice.Models;

namespace practice.Controllers;

public class HomeController : Controller
{

    private readonly AppDbContext _context;

    public HomeController(AppDbContext context) => this._context = context;

    [Route("/")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var list = await _context.Items.ToListAsync();
        // var shuffledList = list.OrderBy(item => new Random().Next()).ToList();
        // var totalCount = await _context.Items.CountAsync();
        // ViewBag.totalPages = (int)Math.Ceiling(totalCount / 4.0);
        // var list = await _context.Items.Skip(0).Take(4).ToListAsync();
        return View(list);
    }

    [Route("/page={page}")]
    [HttpGet]
    public async Task<IActionResult> Index(int page)
    {
        var totalCount = await _context.Items.CountAsync();
        ViewBag.totalPages = (int)Math.Ceiling(totalCount / 4.0);
        var list = await _context.Items.Skip((page - 1) * 4).Take(4).ToListAsync();
        return View(list);
    }

    [Route("/{name}")]
    [HttpGet]
    public async Task<IActionResult> Item(string name)
    {
        name = name.Replace('-', ' ');
        Item? item = await _context.Items.FirstOrDefaultAsync(i => i.Name == name);

        ViewBag.sizes = new SelectList(new int[]{
            40, 41, 42, 43, 44, 45, 46, 47
        });

        return item == null ? NotFound() : View(item);
    }

}
