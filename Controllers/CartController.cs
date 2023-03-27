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
            return View();
        }


        [Route("/Cart/AddToCart")]
        [HttpPost]
        public RedirectResult AddToCart(Item item)
        {
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
            if(checkout.Delivery == Delivery.Dhl)
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