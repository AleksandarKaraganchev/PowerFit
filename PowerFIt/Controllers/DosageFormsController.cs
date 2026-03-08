using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PowerFIt.Data;
using PowerFIt.Models;

namespace PowerFIt.Controllers
{
    public class DosageFormsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DosageFormsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DosageForms
        public async Task<IActionResult> Index()
        {
            return View(await _context.DosageForms.ToListAsync());
        }

        // GET: DosageForms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dosageForm = await _context.DosageForms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dosageForm == null)
            {
                return NotFound();
            }

            return View(dosageForm);
        }

        // GET: DosageForms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DosageForms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] DosageForm dosageForm)
        {
            dosageForm.RegOn = DateTime.Now;
            if (ModelState.IsValid)
            {
                _context.Add(dosageForm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dosageForm);
        }

        // GET: DosageForms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dosageForm = await _context.DosageForms.FindAsync(id);
            if (dosageForm == null)
            {
                return NotFound();
            }
            return View(dosageForm);
        }

        // POST: DosageForms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,RegOn")] DosageForm dosageForm)
        {
            if (id != dosageForm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dosageForm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DosageFormExists(dosageForm.Id))
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
            return View(dosageForm);
        }

        // GET: DosageForms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dosageForm = await _context.DosageForms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dosageForm == null)
            {
                return NotFound();
            }

            return View(dosageForm);
        }

        // POST: DosageForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dosageForm = await _context.DosageForms.FindAsync(id);
            if (dosageForm != null)
            {
                _context.DosageForms.Remove(dosageForm);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DosageFormExists(int id)
        {
            return _context.DosageForms.Any(e => e.Id == id);
        }
    }
}
