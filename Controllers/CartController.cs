using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using practice.Models;
using practice.Auth;
using Stripe.Checkout;
using Stripe;
using practice.Repository;

namespace practice.Controllers
{
    public class CartController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly JWT _jwt;

        public CartController(JWT jwt, IOrderRepository _repository, IUserRepository _userRepository)
        {
            this._jwt = jwt;
            this._orderRepository = _repository;
            this._userRepository = _userRepository;
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
            string? token = HttpContext.Session.GetString("JwtToken");
            if (token != null)
            {
                var currentUser = _jwt.GetCurrentUser(token);
                currentUser = await _userRepository.GetByEmailAsync(currentUser!.Email!);
                if (!string.IsNullOrEmpty(currentUser!.CartItemsJson))
                {
                    int count = HttpContext.Session.GetInt32("LoginedUserProductCount") ?? 0;
                    HttpContext.Session.SetInt32("LoginedUserProductCount", count + 1);
                    var CartList = JsonSerializer.Deserialize<List<Item>>(currentUser.CartItemsJson);
                    CartList!.Add(item);
                    await _userRepository.SetCartItems(currentUser, JsonSerializer.Serialize(CartList));
                    HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(currentUser));
                }
                else
                {
                    int count = HttpContext.Session.GetInt32("LoginedUserProductCount") ?? 0;
                    HttpContext.Session.SetInt32("LoginedUserProductCount", count + 1);
                    var CartList = new List<Item>();
                    CartList.Add(item);
                    await _userRepository.SetCartItems(currentUser, JsonSerializer.Serialize(CartList));
                    HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(currentUser));
                }
            }
            else
            {
                int count = HttpContext.Session.GetInt32("LoginedUserProductCount") ?? 0;
                HttpContext.Session.SetInt32("LoginedUserProductCount", count + 1);
            }
            var list = GetSessionList("cartItems");

            list.Add(item);
            HttpContext.Session.SetString("cartItems", JsonSerializer.Serialize(list));

            int totalPrice = HttpContext.Session.GetInt32("totalPrice") ?? 0;
            totalPrice += item.Price;
            HttpContext.Session.SetInt32("totalPrice", totalPrice);
            return Redirect("/cart");
        }

        [Route("/DeleteItem/{id}")]
        public async Task<RedirectResult> DeleteItem(int id)
        {
            string? token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                var user = await _userRepository.GetByEmailAsync(_jwt.GetCurrentUser(token)!.Email!);
                if (user!.CartItemsJson != null && user.CartItemsJson != "")
                {
                    var CartList = JsonSerializer.Deserialize<List<Item>>(user.CartItemsJson);
                    CartList!.Remove(CartList.Find(x => x.Id == id)!);
                    await _userRepository.SetCartItems(user, JsonSerializer.Serialize(CartList));
                    HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(user));
                }
            }
            var list = GetSessionList("cartItems");

            Item item = list.Find(x => x.Id == id) ?? new Item();
            list.Remove(item);
            HttpContext.Session.SetString("cartItems", JsonSerializer.Serialize(list));

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
            string? token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                var user = await _userRepository.GetByEmailAsync(_jwt.GetCurrentUser(token)!.Email!);
                if (user != null)
                {
                    await _userRepository.SetCartItems(user, null);
                    HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(user!));
                }
            }
            HttpContext.Session.Remove("cartItems");
            HttpContext.Session.Remove("totalPrice");
            HttpContext.Session.Remove("LoginedUserProductCount");

            return Redirect("/cart");
        }


        [Route("/checkout")]
        public IActionResult Checkout()
        {
            var json = HttpContext.Session.GetString("cartItems");
            if (string.IsNullOrEmpty(json))
            {
                return NotFound();
            }
            var currentUser = _jwt.GetCurrentUser(HttpContext.Session.GetString("JwtToken"));
            if (currentUser != null)
            {
                ViewBag.userEmail = currentUser.Email;
                ViewBag.userFirstName = currentUser.FirstName;
                ViewBag.userLastName = currentUser.LastName;
            }
            ViewBag.totalPrice = HttpContext.Session.GetInt32("totalPrice");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Pay(Order order)
        {
            System.Console.WriteLine(order.FirstName + " ++++ " + order.LastName);
            string order_desc = "";
            var checkoutOrder = new Order
            {
                Id = Guid.NewGuid(),
                FirstName = order.FirstName,
                LastName = order.LastName,
                Email = order.Email,
                Town = order.Town,
                Address = order.Address,
                ItemsJson = HttpContext.Session.GetString("cartItems"),
                TotalPrice = order.TotalPrice,
                UserId = 0
            };
            foreach (var item in GetSessionList("cartItems"))
                order_desc += item.Name + ", ";

            string? token = HttpContext.Session.GetString("JwtToken");   
            if (!string.IsNullOrEmpty(token))
            {
                checkoutOrder.UserId = _jwt.GetCurrentUser(token)!.Id;
            }
            await _orderRepository.Add(checkoutOrder);


            StripeConfiguration.ApiKey = "sk_test_51MzK0CFPQmRrRl3KPutq8uBAM0WZ890vWCAj2PKDWCd89zQ3DcCpijZaA0S9IWt59Xz0XvSXrihYQZFVQPKtiL7400oWFzosq6";

            var options = new SessionCreateOptions
            {
               SuccessUrl = $"https://published.bsite.net/checkout/success/{checkoutOrder.Id}",
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
            var order = new Order();
            try
            {
                order = _orderRepository.GetById(id);
                if (order == null)
                    return NotFound();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return NotFound();
            }
            await Clear();
            return View(JsonSerializer.Deserialize<List<Item>>(order.ItemsJson!));
        }

        public List<Item> GetSessionList(string sessionKey)
        {
            var json = HttpContext.Session.GetString(sessionKey);
            var list = new List<Item>();

            if (!string.IsNullOrEmpty(json))
            {
                list = JsonSerializer.Deserialize<List<Item>>(json) ?? new List<Item>();
            }
            return list;
        }
    }
}