using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using practice.Models;
using practice.Repository;

namespace practice.Controllers;

public class HomeController : Controller
{
    private readonly ItemRepository _repository;
    public HomeController(ItemRepository repository) => this._repository = repository;

    [Route("/")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        ViewBag.connString = builder.Configuration.GetConnectionString("DefaultConnection");
        try
        {
            ViewBag.totalPageCount = await _repository.CountAsync();
            var list = await _repository.GetFewAsync(12);
            return View(list);
        }
        catch (Exception ex)
        {
            ViewBag.innerException = ex.InnerException;
            return View();
        }
    }

    [Route("/load={load}")]
    [HttpGet]
    public async Task<IActionResult> Index(int load)
    {
        try
        {
            int totalcount = await _repository.CountAsync();
            ViewBag.totalPageCount = totalcount;
            if (load > totalcount)
                return Redirect($"/load={totalcount}");

            var list = await _repository.GetFewAsync(load);
            return View(list);
        }
        catch (Exception)
        {
            return View();
        }
    }

    [Route("/{name}")]
    [HttpGet]
    public async Task<IActionResult> Item(string name)
    {
        name = name.Replace('-', ' ');
        Item? item = await _repository.GetByNameAsync(name);

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
