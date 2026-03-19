using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerFIt.Data;
using PowerFIt.Models;
using System.Text.Json;

namespace PowerFIt.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Customer> _userManager;
        private const string SessionKey = "CheckoutItems";

        public CheckoutController(ApplicationDbContext context, UserManager<Customer> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var items = GetCheckoutItems();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            if (product.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Продуктът не е наличен.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var items = GetCheckoutItems();
            var existingItem = items.FirstOrDefault(x => x.ProductId == productId);

            var currentQuantityInCheckout = existingItem?.Quantity ?? 0;
            var requestedTotal = currentQuantityInCheckout + quantity;

            if (requestedTotal > product.Quantity)
            {
                TempData["ErrorMessage"] = $"Налични са само {product.Quantity} бр. от продукта.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                items.Add(new CheckoutItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Image = product.Image,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            SaveCheckoutItems(items);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var items = GetCheckoutItems();
            var item = items.FirstOrDefault(x => x.ProductId == productId);

            if (item == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            if (quantity > product.Quantity)
            {
                TempData["StockProductId"] = productId;
                TempData["RequestedQty"] = quantity;
                TempData["AvailableQty"] = product.Quantity;
                TempData["StockMessage"] = $"Налични са само {product.Quantity} бр. от {product.Name}.";
                return RedirectToAction(nameof(Index));
            }

            item.Quantity = quantity;
            SaveCheckoutItems(items);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeOrder()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var items = GetCheckoutItems();
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Няма продукти за поръчка.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var item in items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Един от продуктите вече не съществува.";
                    return RedirectToAction(nameof(Index));
                }

                if (product.Quantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Недостатъчна наличност за {product.Name}. Налични: {product.Quantity} бр.";
                    return RedirectToAction(nameof(Index));
                }
            }

            foreach (var item in items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);

                var order = new Order
                {
                    ProductId = item.ProductId,
                    CustomerId = userId,
                    Quantity = item.Quantity,
                    Description = $"Поръчка за продукт: {item.ProductName}",
                    OrderDate = DateTime.Now
                };

                product.Quantity -= item.Quantity;

                _context.Orders.Add(order);
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(SessionKey);
            TempData["SuccessMessage"] = "Поръчката беше финализирана успешно.";

            return RedirectToAction("Index", "Orders");
        }

        private List<CheckoutItem> GetCheckoutItems()
        {
            var data = HttpContext.Session.GetString(SessionKey);

            if (string.IsNullOrEmpty(data))
            {
                return new List<CheckoutItem>();
            }

            return JsonSerializer.Deserialize<List<CheckoutItem>>(data) ?? new List<CheckoutItem>();
        }

        private void SaveCheckoutItems(List<CheckoutItem> items)
        {
            var data = JsonSerializer.Serialize(items);
            HttpContext.Session.SetString(SessionKey, data);
        }
        public static int GetCartCount(ISession session)
        {
            var data = session.GetString("CheckoutItems");

            if (string.IsNullOrEmpty(data))
                return 0;

            var items = System.Text.Json.JsonSerializer.Deserialize<List<CheckoutItem>>(data);

            return items?.Sum(x => x.Quantity) ?? 0;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmAvailableQuantity(int productId, int quantity)
        {
            var items = GetCheckoutItems();
            var item = items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity < 1 ? 1 : quantity;
                SaveCheckoutItems(items);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var items = GetCheckoutItems();
            var item = items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                items.Remove(item);
                SaveCheckoutItems(items);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

