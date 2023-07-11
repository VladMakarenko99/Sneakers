using Microsoft.AspNetCore.Mvc;
using Sneakers.Data;
using Sneakers.Models;
using Sneakers.Interfaces;

namespace practice.Controllers;

public class FilterController : Controller
{

    private readonly IItemRepository _repository;

    public FilterController(IItemRepository repository) => this._repository = repository;


    [Route("/q:{url}")]
    public async Task<IActionResult> Index(string url)
    {
        var filteredList = await GetEntireList(url);
        ViewBag.totalPageCount = filteredList.Count;

        return View(filteredList.Take(8).ToList());
    }

    [Route("/q:{url}/load={load}")]
    public async Task<IActionResult> Index(string url, int load)
    {
        var filteredList = await GetEntireList(url);
        ViewBag.totalPageCount = filteredList.Count;
        if (load > filteredList.Count)
            return Redirect($"/q:{url}/load={filteredList.Count}");

        return View(filteredList.Take(load).ToList());
    }

    private static int[] GetMinAndMaxPrice(string filter)
    {
        var price = filter.Split("price=")[1];
        var prices = price.Split("-").ToList();
        var min = Convert.ToInt32(prices[0]);
        var max = Convert.ToInt32(prices[1]);
        return new int[] { min, max };
    }

    private async Task<List<Item>> GetFilteredListByBrandAsync(List<Item> listToFilter, string filter)
    {
        var brandList = new List<string>();
        var brandString = filter.Split("brand=")[1];
        if (brandString.Contains("*"))
            brandList = brandString.Split("*").ToList();
        else
            brandList.Add(brandString ?? "");

        var items = new List<Item>();
        foreach (var item in brandList)
        {
            var brand = await _repository.GetBrandByNameAsync(item);

            if (brand != null)
            {
                var items2 = listToFilter.Where(x => x.BrandId == brand.Id).ToList();
                foreach (var i in items2)
                    items.Add(i);
            }

        }
        return items;
    }

    private List<Item> GetFilteredListByCategory(List<Item> listToFilter, string filter)
    {
        var list = new List<string>();
        var filterString = filter.Split("category=")[1];
        if (filterString.Contains("*"))
            list = filterString.Split("*").ToList();
        else
            list.Add(filterString ?? "");

        var items = new List<Item>();
        foreach (var item in list)
        {
            items.AddRange(listToFilter.Where(x => x.BestFor == item));
        }
        return items;
    }


    private async Task<List<Item>> GetEntireList(string url)
    {
        var filteredList = new List<Item>();
        var filterStrings = url.Split("&").ToList();
        var entireList = await _repository.GetAllAsync();

        foreach (string filter in filterStrings)
        {
            if (filter.Contains("brand=") && filteredList.Count == 0 && filterStrings.IndexOf(filter) == 0)
                filteredList = await GetFilteredListByBrandAsync(entireList, filter);

            if (filter.Contains("brand=") && filteredList.Count > 0)
                filteredList = await GetFilteredListByBrandAsync(filteredList, filter);

            if (filter.Contains("category=") && filteredList.Count == 0 && filterStrings.IndexOf(filter) == 0)
                filteredList = GetFilteredListByCategory(entireList, filter);

            if (filter.Contains("category=") && filteredList.Count > 0)
                filteredList = GetFilteredListByCategory(filteredList, filter);

            if (filter.Contains("price=") && filteredList.Count > 0)
            {
                var min = GetMinAndMaxPrice(filter)[0];
                var max = GetMinAndMaxPrice(filter)[1];
                filteredList = filteredList.Where(x => x.Price >= min && x.Price <= max).ToList();
            }

            if (filter.Contains("price=") && filteredList.Count == 0 && filterStrings.IndexOf(filter) == 0)
            {
                var min = GetMinAndMaxPrice(filter)[0];
                var max = GetMinAndMaxPrice(filter)[1];
                filteredList = entireList.Where(x => x.Price >= min && x.Price <= max).ToList();
            }

            if (filter.Contains("sort=") && filteredList.Count > 0)
            {
                var sort = filter.Split("sort=")[1];
                if (sort == "ascending")
                    filteredList = filteredList.OrderBy(x => x.Price).ToList();
                if (sort == "descending")
                    filteredList = filteredList.OrderByDescending(x => x.Price).ToList();
            }
            if (filter.Contains("sort=") && filteredList.Count == 0 && filterStrings.IndexOf(filter) == 0)
            {
                var sort = filter.Split("sort=")[1];
                if (sort == "ascending")
                    filteredList = entireList.OrderBy(x => x.Price).ToList();
                if (sort == "descending")
                    filteredList = entireList.OrderByDescending(x => x.Price).ToList();
            }
            if (filter.Contains("search="))
            {
                string search = filter.Split("search=")[1].ToLower().Replace("*", " ");
                filteredList = entireList.Where(x => x.Name!.ToLower().Contains(search)).ToList();
            }
        }
        return filteredList.Distinct().ToList();
    }
}

