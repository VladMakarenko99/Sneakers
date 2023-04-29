using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using practice.Data;
using practice.Models;
using Stripe.Checkout;
using Stripe;

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
                    int count = HttpContext.Session.GetInt32("LoginedUserProductCount") ?? 0;
                    HttpContext.Session.SetInt32("LoginedUserProductCount", count + 1);
                    var CartList = JsonSerializer.Deserialize<List<Item>>(user.CartItemsJson);
                    CartList!.Add(item);
                    user.CartItemsJson = JsonSerializer.Serialize(CartList);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    int count = HttpContext.Session.GetInt32("LoginedUserProductCount") ?? 0;
                    HttpContext.Session.SetInt32("LoginedUserProductCount", count + 1);
                    var CartList = new List<Item>();
                    CartList.Add(item);
                    user!.CartItemsJson = JsonSerializer.Serialize(CartList);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                int count = HttpContext.Session.GetInt32("LoginedUserProductCount") ?? 0;
                HttpContext.Session.SetInt32("LoginedUserProductCount", count + 1);
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
        public async Task<RedirectResult> DeleteItem(int id)
        {
            if (HttpContext.Session.Get("userEmail") != null)
            {
                var user = _context.Users.FirstOrDefault(x => x.Email == HttpContext.Session.GetString("userEmail"));
                if (user!.CartItemsJson != null && user.CartItemsJson != "")
                {
                    var CartList = JsonSerializer.Deserialize<List<Item>>(user.CartItemsJson);
                    CartList!.Remove(CartList.Find(x => x.Id == id)!);
                    user.CartItemsJson = JsonSerializer.Serialize(CartList);
                    await _context.SaveChangesAsync();
                }
            }
            var list = GetSessionList("cartItems");

            Item item = list.Find(x => x.Id == id) ?? new Item();
            list.Remove(item);
            HttpContext.Session.Set("cartItems", Encoding.UTF8.GetBytes(JsonSerializer.Serialize(list)));

            int totalPrice = HttpContext.Session.GetInt32("totalPrice") ?? 0;
            totalPrice -= item.Price;
            HttpContext.Session.SetInt32("totalPrice", totalPrice);

            int count = HttpContext.Session.GetInt32("LoginedUserProductCount") ?? 0;
            HttpContext.Session.SetInt32("LoginedUserProductCount", count - 1);

            if (!list.Any())
                await Clear();

            return Redirect("/cart");
        }
        public async Task<RedirectResult> Clear()
        {
            if (HttpContext.Session.Get("userEmail") != null)
            {
                var user = _context.Users.FirstOrDefault(x => x.Email == HttpContext.Session.GetString("userEmail"));
                user!.CartItemsJson = null;
                await _context.SaveChangesAsync();
            }
            HttpContext.Session.Remove("cartItems");
            HttpContext.Session.Remove("totalPrice");
            HttpContext.Session.Remove("LoginedUserProductCount");

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
        public async Task<IActionResult> Pay(Order order)
        {
            //string url = "/";
            string order_desc = "";
            var checkoutOrder = new Order
            {
                Id = Guid.NewGuid(),
                FirstName = order.FirstName,
                LastName = order.LastName,
                Email = order.Email,
                Town = order.Town,
                Address = order.Address,
                ItemsJson = GetSessionListJson("cartItems"),
                TotalPrice = order.TotalPrice
            };
            foreach (var item in GetSessionList("cartItems"))
                order_desc += item.Name + ", ";

            _context.Orders.Add(checkoutOrder);
            await _context.SaveChangesAsync();    

            StripeConfiguration.ApiKey = "sk_test_51MzK0CFPQmRrRl3KPutq8uBAM0WZ890vWCAj2PKDWCd89zQ3DcCpijZaA0S9IWt59Xz0XvSXrihYQZFVQPKtiL7400oWFzosq6";

            var options = new SessionCreateOptions
            {
                SuccessUrl = $"http://localhost:8000/checkout/success/{checkoutOrder.Id}",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = checkoutOrder.TotalPrice * 100,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions{
                                Name = order_desc.TrimEnd(new char[2]{',', ' '}),
                                Description = "Payment for sneakers"
                            }
                        }
                    },
                },
                CustomerEmail = checkoutOrder.Email,
                Mode = "payment",
            };
            var service = new SessionService();
            var intent = service.Create(options);

            Response.Headers.Add("Location", intent.Url);
            
            return Redirect(intent.Url);
        }

        [Route("/checkout/success/{id}")]
        public async Task<IActionResult> Success(string id)
        {
            await Clear();
            return View();
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

        public string GetSessionListJson(string sessionKey){
            var bytes = HttpContext.Session.Get(sessionKey) ?? Array.Empty<byte>();
            string json = Encoding.UTF8.GetString(bytes);

            return json;
        }
    }
}