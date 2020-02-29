using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusService.Models;
using Microsoft.AspNetCore.Http;

namespace BusService.Controllers
{
    public class RouteStopController : Controller
    {
        private readonly BusServiceContext _context;

        public RouteStopController(BusServiceContext context)
        {
            _context = context;
        }

        // GET: RouteStop
        public async Task<IActionResult> Index(string BusRouteCode)
        {
            if(BusRouteCode != null)
            {
                HttpContext.Session.SetString("BusRouteCode", BusRouteCode);
            }
            else if (Request.Query["BusRouteCode"].Any())
            {
                HttpContext.Session.SetString("BusRouteCode", Request.Query["BusRouteCode"]);
            }
            else if (HttpContext.Session.GetString("BusRouteCode")!= null)
            {
                HttpContext.Session.SetString("BusRouteCode", BusRouteCode);
            }
            else
            {
                TempData["Message"] = "Please Select Route First";
                return RedirectToAction("Index","BusRoute");
            }
            var busRoute = _context.BusRoute.Where(a=>a.BusRouteCode==BusRouteCode).FirstOrDefault();
            ViewData["BusRouteCode"] = busRoute.BusRouteCode;
            ViewData["BusRouteName"] = busRoute.RouteName;
            var busServiceContext = _context.RouteStop.Include(r => r.BusRouteCodeNavigation).Include(r => r.BusStopNumberNavigation).OrderBy(a=>a.OffsetMinutes);
            return View(await busServiceContext.ToListAsync());
        }

        // GET: RouteStop/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop
                .Include(r => r.BusRouteCodeNavigation)
                .Include(r => r.BusStopNumberNavigation)
                .FirstOrDefaultAsync(m => m.RouteStopId == id);
            if (routeStop == null)
            {
                return NotFound();
            }

            return View(routeStop);
        }

        // GET: RouteStop/Create
        public IActionResult Create()
        {
            string busRCode = string.Empty;
            if(HttpContext.Session.GetString("BusRouteCode") != null)
            {
                busRCode = HttpContext.Session.GetString("BusRouteCode");
            }
            var busRoute = _context.BusRoute.Where(a => a.BusRouteCode == busRCode).FirstOrDefault();
            ViewData["BusRCode"] = busRoute.BusRouteCode;
            ViewData["BusRName"] = busRoute.RouteName;
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode");
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop.OrderBy(a=>a.Location), "BusStopNumber", "Location");
            return View();
        }

        // POST: RouteStop/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RouteStopId,BusRouteCode,BusStopNumber,OffsetMinutes")] RouteStop routeStop)
        {
            string busRCode = string.Empty;
            if (HttpContext.Session.GetString("BusRouteCode") != null)
            {
                busRCode = HttpContext.Session.GetString("BusRouteCode");
            }
            if(routeStop.OffsetMinutes < 0)
            {
                ModelState.AddModelError("","Offset Minute Must Be Zero");
            }
            var busRoute = _context.BusRoute.Where(a => a.BusRouteCode == busRCode).FirstOrDefault();
            ViewData["BusRCode"] = busRoute.BusRouteCode;
            ViewData["BusRName"] = busRoute.RouteName;

            routeStop.BusRouteCode = busRCode;
            var isZeroExist = _context.RouteStop.Where(a => a.OffsetMinutes == 0 && a.BusRouteCode == routeStop.BusRouteCode);
            if (isZeroExist.Any())
            {
                ModelState.AddModelError("", "There is already record  for offsetminutes as zero");
            }
            var isDuplicate = _context.RouteStop.Where(a => a.BusRouteCode == routeStop.BusRouteCode && a.BusStopNumber == routeStop.BusStopNumber);
            if (isDuplicate.Any())
            {
                ModelState.AddModelError("", "Duplicate Recored");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(routeStop);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "New route stop Added";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.GetBaseException().Message);
                }
            }
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeStop.BusRouteCode);
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop, "BusStopNumber", "Location", routeStop.BusStopNumber);
            return View(routeStop);
        }

        // GET: RouteStop/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop.FindAsync(id);
            if (routeStop == null)
            {
                return NotFound();
            }
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeStop.BusRouteCode);
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop, "BusStopNumber", "BusStopNumber", routeStop.BusStopNumber);
            return View(routeStop);
        }

        // POST: RouteStop/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RouteStopId,BusRouteCode,BusStopNumber,OffsetMinutes")] RouteStop routeStop)
        {
            if (id != routeStop.RouteStopId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(routeStop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteStopExists(routeStop.RouteStopId))
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
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeStop.BusRouteCode);
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop, "BusStopNumber", "BusStopNumber", routeStop.BusStopNumber);
            return View(routeStop);
        }

        // GET: RouteStop/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop
                .Include(r => r.BusRouteCodeNavigation)
                .Include(r => r.BusStopNumberNavigation)
                .FirstOrDefaultAsync(m => m.RouteStopId == id);
            if (routeStop == null)
            {
                return NotFound();
            }

            return View(routeStop);
        }

        // POST: RouteStop/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var routeStop = await _context.RouteStop.FindAsync(id);
            _context.RouteStop.Remove(routeStop);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RouteStopExists(int id)
        {
            return _context.RouteStop.Any(e => e.RouteStopId == id);
        }
    }
}
