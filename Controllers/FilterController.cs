using Microsoft.AspNetCore.Mvc;
using practice.Data;
using practice.Models;
using System.Text.RegularExpressions;

namespace practice.Controllers;

public class FilterController : Controller
{

    private readonly AppDbContext _context;

    public FilterController(AppDbContext context) => this._context = context;




    [Route("/q:{url}")]
    public IActionResult Index(string url)
    {
        System.Console.WriteLine(url);
        var filteredList = new List<Item>();
        var filterStrings = url.Split("&").ToList();
        System.Console.WriteLine(filterStrings.FindIndex(s => new Regex(@"brand=.+s").Match(s).Success));

        foreach (string filter in filterStrings)
        {
            if (filter.Contains("brand=") && filteredList.Count == 0 && filterStrings.IndexOf(filter) == 0) 
                filteredList = GetFilteredListByBrand(_context.Items.ToList(), filter);
        
            if (filter.Contains("brand=") && filteredList.Count > 0)
                filteredList = GetFilteredListByBrand(filteredList, filter);
            
            if (filter.Contains("price=") && filteredList.Count > 0)
            {
                System.Console.WriteLine("FILTEERING FROM LIST");
                var min = GetMinAndMaxPrice(filter)[0];
                var max = GetMinAndMaxPrice(filter)[1];
                filteredList = filteredList.Where(x => x.Price >= min && x.Price <= max).ToList();
            }

            if (filter.Contains("price=") && filteredList.Count == 0 &&  filterStrings.IndexOf(filter) == 0)
            {
                System.Console.WriteLine("FILTERING FROM DATABSE");
                var min = GetMinAndMaxPrice(filter)[0];
                var max = GetMinAndMaxPrice(filter)[1];
                filteredList = _context.Items.Where(x => x.Price >= min && x.Price <= max).ToList();
            }

            if(filter.Contains("sort=") && filteredList.Count > 0)
            {
                var sort = filter.Split("sort=")[1];
                if(sort == "ascending")
                    filteredList = filteredList.OrderBy(x => x.Price).ToList();
                if(sort == "descending")
                    filteredList = filteredList.OrderByDescending(x => x.Price).ToList();
            }
            if(filter.Contains("sort=") && filteredList.Count == 0 && filterStrings.IndexOf(filter) == 0)
            {
                var sort = filter.Split("sort=")[1];
                if(sort == "ascending")
                    filteredList = _context.Items.OrderBy(x => x.Price).ToList();
                if(sort == "descending")
                    filteredList = _context.Items.OrderByDescending(x => x.Price).ToList();
            }
            if(filter.Contains("search=")){
                string search = filter.Split("search=")[1].ToLower().Replace("-", " ");
                filteredList = _context.Items.Where(x => x.Name!.ToLower().Contains(search)).ToList();
            
            }
        }

        return View(filteredList.Distinct().ToList());
    }


    public static int[] GetMinAndMaxPrice(string filter)
    {
        var price = filter.Split("price=")[1];
        var prices = price.Split("-").ToList();
        var min = Convert.ToInt32(prices[0]);
        var max = Convert.ToInt32(prices[1]);
        return new int[] { min, max };
    }

    public List<Item> GetFilteredListByBrand(List<Item> listToFilter, string filter)
    {
        var brandList = new List<string>();
        var brandString = filter.Split("brand=")[1];
        if (brandString.Contains("+"))
            brandList = brandString.Split("+").ToList();
        else
            brandList.Add(brandString ?? "");

        var items = new List<Item>();
        foreach (var item in brandList)
        {
            var brand = _context.Brands.FirstOrDefault(x => x.Name == item.Replace("-", " "));

            if (brand != null)
            {
                var items2 = listToFilter.Where(x => x.BrandId == brand.Id).ToList();
                foreach (var i in items2)
                    items.Add(i);
            }

        }
        return items;
    }
}

