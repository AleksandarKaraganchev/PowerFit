using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerFIt.Data;
using PowerFIt.Models;

namespace PowerFIt.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<Customer> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<Customer> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var orders = await _context.Orders
                .Include(o => o.Products)
                .Where(o => o.CustomerId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.UserOrders = orders;

            return View(user);
        }
    }
}