using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerFIt.Data;

namespace PowerFIt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ProductsCount = await _context.Products.CountAsync();
            ViewBag.CategoriesCount = await _context.Categories.CountAsync();
            ViewBag.DosageFormsCount = await _context.DosageForms.CountAsync();
            ViewBag.OrdersCount = await _context.Orders.CountAsync();
            ViewBag.UsersCount = await _context.Users.CountAsync();

            var lowStockProducts = await _context.Products
                .Where(p => p.Quantity <= 5)
                .OrderBy(p => p.Quantity)
                .Take(5)
                .ToListAsync();

            ViewBag.LowStockProducts = lowStockProducts;

            return View();
        }
    }
}