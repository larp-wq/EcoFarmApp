using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Обязательно добавить!
using Microsoft.EntityFrameworkCore;
using EcoFarmApp.Data;
using EcoFarmApp.Models;

namespace EcoFarmApp.Controllers
{
    public class SalesController : Controller
    {
        private readonly EcoFarmDbContext _context;

        public SalesController(EcoFarmDbContext context)
        {
            _context = context;
        }

        // GET: Sales
        public async Task<IActionResult> Index()
        {
            // Отображаем историю продаж. Включаем информацию о продукте для отображения названия
            var ecoFarmDbContext = _context.Sales.Include(s => s.Product);
            return View(await ecoFarmDbContext.ToListAsync());
        }

        // GET: Sales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Включаем продукт для отображения названия
            var sale = await _context.Sales
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.SaleID == id);
            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        // GET: Sales/Create
        public async Task<IActionResult> Create()
        {
            // Это заполняет список продуктов для выпадающего списка в форме
            ViewData["Products"] = new SelectList(await _context.Products.ToListAsync(), "ProductID", "ProductName");
            return View();
        }

        // Этот метод обрабатывает отправку формы создания продажи
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SaleID,ProductID,Quantity,CustomerName,SaleDate")] Sale sale)
        {
            ViewData["Products"] = new SelectList(await _context.Products.ToListAsync(), "ProductID", "ProductName", sale?.ProductID);

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage + (e.Exception != null ? " (ex: " + e.Exception.Message + ")" : "")));
                ModelState.AddModelError(string.Empty, "ModelState errors: " + errors);
                return View(sale); // Возвращаем форму с ошибками
            }

            // Находим продукт
            var product = await _context.Products.FindAsync(sale.ProductID);
            if (product == null)
            {
                ModelState.AddModelError(string.Empty, "Выбранный продукт не найден.");
                return View(sale); // Возвращаем форму с ошибкой
            }

            // Проверяем количество на складе продукта
            var saleQty = sale.Quantity;
            if (product.StockQuantity < saleQty)
            {
                ModelState.AddModelError(string.Empty, "Недостаточное количество продукта на складе.");
                return View(sale); // Возвращаем форму с ошибкой
            }

            // Используем транзакцию для атомарности операций (списание и запись продажи)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Списываем со склада продукта
                product.StockQuantity -= saleQty;
                _context.Products.Update(product);

                // Если у продукта есть связанный InventoryItem (сырьё) — списываем и оттуда
                if (product.InventoryItemId.HasValue)
                {
                    var inv = await _context.InventoryItems.FindAsync(product.InventoryItemId.Value);
                    if (inv == null)
                    {
                        ModelState.AddModelError(string.Empty, "Связанная складская позиция (сырьё) не найдена.");
                        await transaction.RollbackAsync(); // Отменяем все изменения
                        return View(sale); // Возвращаем форму с ошибкой
                    }
                    if (inv.Quantity < saleQty) // Можешь решить, списывать ли 1:1 или по другому коэффициенту
                    {
                        ModelState.AddModelError(string.Empty, $"Недостаточно сырья '{inv.ItemName}' на складе для производства.");
                        await transaction.RollbackAsync(); // Отменяем все изменения
                        return View(sale); // Возвращаем форму с ошибкой
                    }
                    inv.Quantity -= saleQty;
                    _context.InventoryItems.Update(inv);
                }

                // Добавляем запись о продаже
                _context.Sales.Add(sale);

                // Сохраняем все изменения в базе данных
                await _context.SaveChangesAsync();

                // Если все успешно — фиксируем транзакцию
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index)); // Перенаправляем на список продаж
            }
            catch (Exception ex)
            {
                // Если произошла ошибка при сохранении — откатываем транзакцию
                ModelState.AddModelError(string.Empty, "Ошибка сохранения продажи: " + ex.Message);
                try { await transaction.RollbackAsync(); } catch { } // Откатываем транзакцию, если возможно
                return View(sale); // Возвращаем форму с сообщением об ошибке
            }
        }

        // GET: Sales/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }
            ViewData["ProductID"] = new SelectList(await _context.Products.ToListAsync(), "ProductID", "ProductName", sale.ProductID);
            return View(sale);
        }

        // POST: Sales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SaleID,ProductID,Quantity,CustomerName,SaleDate")] Sale sale)
        {
            if (id != sale.SaleID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SaleExists(sale.SaleID))
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
            ViewData["ProductID"] = new SelectList(await _context.Products.ToListAsync(), "ProductID", "ProductName", sale.ProductID);
            return View(sale);
        }

        // GET: Sales/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _context.Sales
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.SaleID == id);
            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale != null)
            {
                _context.Sales.Remove(sale);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.SaleID == id);
        }
    }
}
