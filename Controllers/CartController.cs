using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sneakers.Models;
using Sneakers.Auth;
using Stripe.Checkout;
using Stripe;
using Sneakers.Interfaces;

namespace practice.Controllers
{
    public class CartController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        private readonly Auth _auth;


        public CartController(IOrderRepository _repository, IUserRepository _userRepository, Auth _auth)
        {

            this._auth = _auth;
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
            if (User.Identity!.IsAuthenticated)
            {
                var currentUser = _auth.GetCurrentUser(HttpContext);
                currentUser = await _userRepository.GetByEmailAsync(currentUser!.Email!);
                if (!string.IsNullOrEmpty(currentUser!.CartItemsJson))
                {
                    int count = HttpContext.Session.GetInt32("LoginedUserProductCount") ?? 0;
                    HttpContext.Session.SetInt32("LoginedUserProductCount", count + 1);
                    var CartList = JsonSerializer.Deserialize<List<Item>>(currentUser.CartItemsJson);
                    CartList!.Add(item);
                    await _userRepository.SetCartItems(currentUser, JsonSerializer.Serialize(CartList));

                    await _auth.SignIn(currentUser, _userRepository, HttpContext);
                }
                else
                {
                    int count = HttpContext.Session.GetInt32("LoginedUserProductCount") ?? 0;
                    HttpContext.Session.SetInt32("LoginedUserProductCount", count + 1);
                    var CartList = new List<Item>();
                    CartList.Add(item);
                    await _userRepository.SetCartItems(currentUser, JsonSerializer.Serialize(CartList));

                    await _auth.SignIn(currentUser, _userRepository, HttpContext);
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
            if (User.Identity!.IsAuthenticated)
            {
                var user = _auth.GetCurrentUser(HttpContext);
                if (!string.IsNullOrEmpty(user.CartItemsJson))
                {
                    var CartList = JsonSerializer.Deserialize<List<Item>>(user.CartItemsJson);
                    CartList!.Remove(CartList.Find(x => x.Id == id)!);
                    await _userRepository.SetCartItems(user, JsonSerializer.Serialize(CartList));

                    await _auth.SignIn(user, _userRepository, HttpContext);
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
            if (User.Identity!.IsAuthenticated)
            {
                var user = await _userRepository.GetByEmailAsync(_auth.GetCurrentUser(HttpContext)!.Email!);
                if (user != null)
                {
                    await _userRepository.SetCartItems(user, null);
                    await _auth.SignIn(user, _userRepository, HttpContext);
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
                return Redirect("/");

            var currentUser = _auth.GetCurrentUser(HttpContext);
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

            if (User.Identity!.IsAuthenticated)
            {
                checkoutOrder.UserId = _auth.GetCurrentUser(HttpContext)!.Id;
            }
            await _orderRepository.Add(checkoutOrder);

            string successUrl = order.SuccessUrl + checkoutOrder.Id;

            StripeConfiguration.ApiKey = "sk_test_51MzK0CFPQmRrRl3KPutq8uBAM0WZ890vWCAj2PKDWCd89zQ3DcCpijZaA0S9IWt59Xz0XvSXrihYQZFVQPKtiL7400oWFzosq6";

            var options = new SessionCreateOptions
            {
                SuccessUrl = successUrl,
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