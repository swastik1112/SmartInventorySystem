using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartInventorySystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Audit/Index
        public async Task<IActionResult> Index(
            string? search = null,
            string? entity = null,
            string? actionType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1)
        {
            const int PageSize = 25;

            var query = _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(l =>
                    l.UserId.ToLower().Contains(search) ||
                    l.Action.ToLower().Contains(search) ||
                    l.Entity.ToLower().Contains(search) ||
                    l.Details.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(entity))
            {
                query = query.Where(l => l.Entity == entity);
            }

            if (!string.IsNullOrWhiteSpace(actionType))
            {
                query = query.Where(l => l.Action == actionType);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(l => l.Timestamp >= dateFrom.Value.Date);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(l => l.Timestamp < dateTo.Value.Date.AddDays(1));
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Pass data to view
            ViewBag.Search = search;
            ViewBag.Entity = entity;
            ViewBag.ActionType = actionType;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.Page = page;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = PageSize;

            return View(logs);
        }

        // POST: Clear all logs (with confirmation in view)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            _context.AuditLogs.RemoveRange(_context.AuditLogs);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Audit log history has been cleared.";
            return RedirectToAction(nameof(Index));
        }
    }
}