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
        ViewBag.totalPageCount = await _context.Items.CountAsync();
        var list = await _context.Items.Take(12).ToListAsync();
        return View(list);
    }

    [Route("/load={count}")]
    [HttpGet]
    public async Task<IActionResult> Index(int count)
    {
        int totalcount = await _context.Items.CountAsync();
        ViewBag.totalPageCount = totalcount;
        if (count > totalcount)
            return Redirect($"/load={totalcount}");

        var list = await _context.Items.Take(count).ToListAsync();
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


    [Route("/api/images/{imageName}")]
    [HttpGet]
    public IActionResult GetImage(string imageName)
    {
        imageName = imageName.Replace('-', ' ');
        byte[] imageBytes = System.IO.File.ReadAllBytes($"wwwroot/img/{imageName}");
        string contentType = "image/jpeg";
        if (imageName.Contains("svg"))
            contentType = "image/svg+xml";
        else if (imageName.Contains("png"))
            contentType = "image/png";

        return File(imageBytes, contentType);
    }
}
