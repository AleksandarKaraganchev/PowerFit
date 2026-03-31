using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PowerFIt.Data;
using PowerFIt.Models;

namespace PowerFIt.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Customer> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<Customer> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string searchTerm, string dateFilter)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Products)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                ordersQuery = ordersQuery.Where(o => o.CustomerId == userId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.Products != null &&
                    o.Products.Name.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(dateFilter))
            {
                var today = DateTime.Today;

                ordersQuery = dateFilter switch
                {
                    "today" => ordersQuery.Where(o => o.OrderDate.Date == today),
                    "week" => ordersQuery.Where(o => o.OrderDate >= today.AddDays(-7)),
                    "month" => ordersQuery.Where(o => o.OrderDate >= today.AddMonths(-1)),
                    _ => ordersQuery
                };
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.DateFilter = dateFilter;

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                if (order.CustomerId != userId)
                {
                    return Forbid();
                }
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            //ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "Name");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Quantity,Description")] Order order)
        {
            if (order.Quantity <= 0)
            {
                order.Quantity = 1;
            }

            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                order.CustomerId = _userManager.GetUserId(User);
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", order.ProductId);
            return View(order);
        }

        // GET: Orders/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                if (order.CustomerId != userId)
                {
                    return Forbid();
                }
            }

            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "UserName", order.CustomerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", order.ProductId);
            return View(order);
        }
        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,Quantity,Description")] Order order)
        {
            order.OrderDate = DateTime.Now;

            if (order.Quantity <= 0)
            {
                order.Quantity = 1;
            }

            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    order.CustomerId = _userManager.GetUserId(User);
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", order.ProductId);
            return View(order);
        }

        // GET: Orders/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                if (order.CustomerId != userId)
                {
                    return Forbid();
                }
            }

            return View(order);
        }


        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                if (order.CustomerId != userId)
                {
                    return Forbid();
                }
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrderFromProduct(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                quantity = 1;
            }

            var order = new Order
            {
                ProductId = product.Id,
                CustomerId = userId,
                Quantity = quantity,
                Description = $"Поръчка за продукт: {product.Name}",
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
