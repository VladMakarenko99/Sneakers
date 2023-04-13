using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using practice.Data;
using practice.Models;
using CloudIpspSDK;
using CloudIpspSDK.Checkout;

namespace practice.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            this._context = context;
        }

        [Route("/cart")]
        public IActionResult Index()
        {
            ViewBag.totalPrice = HttpContext.Session.GetInt32("totalPrice");
            string? email = HttpContext.Session.GetString("userEmail");
            if (email != null)
            {
                var user = _context.Users.FirstOrDefault(x => x.Email == email);
                if (user!.CartItemsJson != null)
                {
                    var CartList = JsonSerializer.Deserialize<List<Item>>(user.CartItemsJson);
                    HttpContext.Session.Set("cartItems", Encoding.UTF8.GetBytes(JsonSerializer.Serialize(CartList)));
                }
                return View(GetSessionList("cartItems"));
            }

            return View(GetSessionList("cartItems"));
        }


        [Route("/Cart/AddToCart")]
        [HttpPost]
        public async Task<RedirectResult> AddToCart(Item item)
        {
            if (HttpContext.Session.Get("userEmail") != null)
            {
                var user = _context.Users.FirstOrDefault(x => x.Email == HttpContext.Session.GetString("userEmail"));
                if (user!.CartItemsJson != null && user.CartItemsJson != "")
                {
                    System.Console.WriteLine("GET IN");
                    var CartList = JsonSerializer.Deserialize<List<Item>>(user.CartItemsJson);
                    CartList!.Add(item);
                    user.CartItemsJson = JsonSerializer.Serialize(CartList);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    System.Console.WriteLine("GET OUT");
                    var CartList = new List<Item>();
                    CartList.Add(item);
                    user!.CartItemsJson = JsonSerializer.Serialize(CartList);
                    await _context.SaveChangesAsync();
                }
            }
            var list = GetSessionList("cartItems");

            list.Add(item);
            HttpContext.Session.Set("cartItems", Encoding.UTF8.GetBytes(JsonSerializer.Serialize(list)));

            int totalPrice = HttpContext.Session.GetInt32("totalPrice") ?? 0;
            totalPrice += item.Price;
            HttpContext.Session.SetInt32("totalPrice", totalPrice);
            return Redirect("/cart");
        }

        [Route("/DeleteItem/{id}")]
        public RedirectResult DeleteItem(int id)
        {
            if(HttpContext.Session.Get("userEmail") != null)
            {
                var user = _context.Users.FirstOrDefault(x => x.Email == HttpContext.Session.GetString("userEmail"));
                if (user!.CartItemsJson != null && user.CartItemsJson != "")
                {
                    var CartList = JsonSerializer.Deserialize<List<Item>>(user.CartItemsJson);
                    CartList!.Remove(CartList.Find(x => x.Id == id)!);
                    user.CartItemsJson = JsonSerializer.Serialize(CartList);
                    _context.SaveChanges();
                }
            }
            var list = GetSessionList("cartItems");

            Item item = list.Find(x => x.Id == id) ?? new Item();
            list.Remove(item);
            HttpContext.Session.Set("cartItems", Encoding.UTF8.GetBytes(JsonSerializer.Serialize(list)));

            int totalPrice = HttpContext.Session.GetInt32("totalPrice") ?? 0;
            totalPrice -= item.Price;
            HttpContext.Session.SetInt32("totalPrice", totalPrice);

            if (!list.Any())
                Clear();

            return Redirect("/cart");
        }
        public RedirectResult Clear()
        {
            HttpContext.Session.Remove("cartItems");
            HttpContext.Session.Remove("totalPrice");

            return Redirect("/cart");
        }


        [Route("/checkout")]
        public IActionResult Checkout()
        {
            var bytes = HttpContext.Session.Get("cartItems");
            if (bytes == null)
            {
                return NotFound();
            }
            ViewBag.userEmail = HttpContext.Session.GetString("userEmail");
            ViewBag.userFirstName = HttpContext.Session.GetString("userFirstName");
            ViewBag.userLastName = HttpContext.Session.GetString("userLastName");
            ViewBag.totalPrice = HttpContext.Session.GetInt32("totalPrice");

            return View();
        }

        [HttpPost]
        public RedirectResult Pay(Order order)
        {
            string url = "/";
            string order_desc = "";
            var checkout = new Order
            {
                Id = Guid.NewGuid(),
                FirstName = order.FirstName,
                LastName = order.LastName,
                Email = order.Email,
                Town = order.Town,
                Address = order.Address,
                Delivery = order.Delivery,
                Items = GetSessionList("cartItems"),
                TotalPrice = order.TotalPrice
            };
            if (checkout.Delivery == Delivery.Dhl)
                checkout.TotalPrice += 12;

            foreach (var item in checkout.Items)
                order_desc += item.Name + $"(Size: {item.Size}), ";


            Config.MerchantId = 1396424;
            Config.SecretKey = "test";

            var req = new CheckoutRequest
            {
                order_id = Guid.NewGuid().ToString("N"),
                amount = Convert.ToInt32(checkout.TotalPrice) * 100,
                order_desc = order_desc.Substring(0, order_desc.Length - 2),
                currency = "USD",
                lang = "en",
                sender_email = checkout.Email
            };
            var resp = new Url().Post(req);
            if (resp.Error == null)
            {
                url = resp.checkout_url;
            }
            return Redirect(url);
        }

        public List<Item> GetSessionList(string sessionKey)
        {
            var bytes = HttpContext.Session.Get(sessionKey) ?? Array.Empty<byte>();
            string json = Encoding.UTF8.GetString(bytes);
            var list = new List<Item>();

            if (!string.IsNullOrEmpty(json))
            {
                list = JsonSerializer.Deserialize<List<Item>>(json) ?? new List<Item>();
            }
            return list;
        }
    }
}