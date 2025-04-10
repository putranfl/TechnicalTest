using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechnicalTest.Models;
using System.Text.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Linq;



namespace TechnicalTest.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string keyword, DateTime? orderDate, int page = 1)
        {
            int pageSize = 5;

            var query = _context.SoOrders
                .Include(x => x.ComCustomer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    x.OrderNo.Contains(keyword) ||
                    x.ComCustomer.CustomerName.Contains(keyword));
            }

            if (orderDate.HasValue)
            {
                query = query.Where(x => x.OrderDate.Date == orderDate.Value.Date);
            }

            int totalItems = await query.CountAsync();

            var orders = await query
                .OrderByDescending(x => x.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new OrderListViewModel
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Keyword = keyword,
                OrderDate = orderDate
            };

            return View(model);
        }



        // GET: Orders/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var soOrder = await _context.SoOrders
                .FirstOrDefaultAsync(m => m.SoOrderId == id);
            if (soOrder == null)
            {
                return NotFound();
            }

            return View(soOrder);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewBag.Customers = _context.ComCustomers
                .Select(c => new SelectListItem
                {
                    Value = c.ComCustomerId.ToString(),
                    Text = c.CustomerName
                }).ToList();

            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SoOrder soOrder, string ItemsJson)
        {
            if (ModelState.IsValid)
            {
                _context.SoOrders.Add(soOrder);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(ItemsJson))
                {
                    var items = JsonSerializer.Deserialize<List<SoItem>>(ItemsJson);

                    foreach (var item in items)
                    {
                        item.SoOrderId = soOrder.SoOrderId; // foreign key
                        _context.SoItems.Add(item);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = new SelectList(_context.ComCustomers, "ComCustomerId", "Name");
            return View(soOrder);
        }




        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var soOrder = await _context.SoOrders.FindAsync(id);
            if (soOrder == null)
            {
                return NotFound();
            }

            ViewBag.Customers = _context.ComCustomers
                .Select(c => new SelectListItem
                {
                    Value = c.ComCustomerId.ToString(),
                    Text = c.CustomerName
                }).ToList();

            // Ambil item lama dan lempar ke view dalam bentuk JSON
            var items = await _context.SoItems
                .Where(i => i.SoOrderId == soOrder.SoOrderId)
                .ToListAsync();
            ViewBag.ItemsJson = JsonSerializer.Serialize(items);

            return View(soOrder);
        }


        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, SoOrder soOrder, string ItemsJson)
        {
            if (id != soOrder.SoOrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(soOrder);
                    await _context.SaveChangesAsync();

                    // Hapus item lama
                    var oldItems = _context.SoItems.Where(i => i.SoOrderId == id);
                    _context.SoItems.RemoveRange(oldItems);
                    await _context.SaveChangesAsync();

                    // Tambahkan item baru dari JSON
                    if (!string.IsNullOrEmpty(ItemsJson))
                    {
                        var items = JsonSerializer.Deserialize<List<SoItem>>(ItemsJson);
                        foreach (var item in items)
                        {
                            item.SoOrderId = soOrder.SoOrderId;
                            _context.SoItems.Add(item);
                        }
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SoOrderExists(soOrder.SoOrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Jika ModelState tidak valid
            // Tambahkan log error biar tahu masalahnya
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"[Model Error] {key}: {error.ErrorMessage}");
                }
            }

            // Isi ulang dropdown dan items biar view ga error
            ViewBag.Customers = _context.ComCustomers
                .Select(c => new SelectListItem
                {
                    Value = c.ComCustomerId.ToString(),
                    Text = c.CustomerName
                }).ToList();

            // Jika gagal, tampilkan kembali item yang sudah disubmit (optional, supaya tidak hilang di UI)
            if (!string.IsNullOrEmpty(ItemsJson))
            {
                ViewBag.ItemsJson = ItemsJson;
            }
            else
            {
                // fallback: ambil dari database
                var existingItems = await _context.SoItems
                    .Where(i => i.SoOrderId == soOrder.SoOrderId)
                    .ToListAsync();
                ViewBag.ItemsJson = JsonSerializer.Serialize(existingItems);
            }

            return View(soOrder);
        }



        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var soOrder = await _context.SoOrders
                .FirstOrDefaultAsync(m => m.SoOrderId == id);
            if (soOrder == null)
            {
                return NotFound();
            }

            return View(soOrder);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var soOrder = await _context.SoOrders.FindAsync(id);
            if (soOrder != null)
            {
                _context.SoOrders.Remove(soOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SoOrderExists(long id)
        {
            return _context.SoOrders.Any(e => e.SoOrderId == id);
        }

        public IActionResult Export(string keyword, DateTime? orderDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var orders = _context.SoOrders
                .Include(o => o.ComCustomer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                orders = orders.Where(o =>
                    o.OrderNo.Contains(keyword) ||
                    o.ComCustomer.CustomerName.Contains(keyword));
            }

            if (orderDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Date == orderDate.Value.Date);
            }

            var list = orders.ToList();


            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Orders");

                // Header
                worksheet.Cells[1, 1].Value = "Order No";
                worksheet.Cells[1, 2].Value = "Customer Name";
                worksheet.Cells[1, 3].Value = "Order Date";
                worksheet.Cells[1, 4].Value = "Address";
                worksheet.Row(1).Style.Font.Bold = true;

                // Data
                for (int i = 0; i < list.Count; i++)
                {
                    var order = list[i];
                    worksheet.Cells[i + 2, 1].Value = order.OrderNo;
                    worksheet.Cells[i + 2, 2].Value = order.ComCustomer?.CustomerName;
                    worksheet.Cells[i + 2, 3].Value = order.OrderDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 4].Value = order.Address;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrderList.xlsx");
            }
        }
    }
}
