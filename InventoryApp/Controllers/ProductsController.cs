using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Models;
using InventoryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace InventoryApp.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private ITrafficCaseRepository caseRepository;
        private CloudQueueClient queueClient;
        private ICacheClient cacheClient;

        public ProductsController(CloudQueueClient queueClient, ICacheClient cacheClient, ITrafficCaseRepository caseRepository)
        {
            this.queueClient = queueClient;
            this.cacheClient = cacheClient;
            this.caseRepository = caseRepository;
        }

        // GET: Cases
        public async Task<IActionResult> Index()
        {
            var cases = await this.caseRepository.GetCases();
            return View(cases);
        }

        //// GET: Products/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _context.Products
        //        .SingleOrDefaultAsync(m => m.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "Case")]TrafficCase trafficCase)
        {
            CaseViewModel cvModel = new CaseViewModel();
            if (ModelState.IsValid)
            {
                //create case method
                this.caseRepository.CreateCollection();
                await this.caseRepository.CreateCase(trafficCase);
               // Restock(product);
                return RedirectToAction(nameof(Index));
            }
            cvModel.Case = trafficCase;
            cvModel.Statuses = this.cacheClient.GetStatuses().ToSelectList();
            return View(cvModel);
        }

        // GET: Products/Edit/5
        public IActionResult Edit(string id)
        {
            CaseViewModel cvModel = new CaseViewModel();
            if (id == null)
            {
                return NotFound();
            }

            var trafficCase = this.caseRepository.GetCase(id);
            if (trafficCase == null)
            {
                return NotFound();
            }
            cvModel.Case = trafficCase;
            cvModel.Statuses = this.cacheClient.GetStatuses().ToSelectList();
            return View(cvModel);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind(Prefix = "Case")]TrafficCase trafficCase)
        {
            CaseViewModel cvModel = new CaseViewModel();
            if (!this.ModelState.IsValid)
            {
                cvModel.Case = trafficCase;
                cvModel.Statuses = this.cacheClient.GetStatuses().ToSelectList();
                return View(cvModel);
            }
            else
            {
                this.caseRepository.EditCase(trafficCase);
              //  Restock(product);
                return this.RedirectToAction("Index");
            }
        }

        //// GET: Products/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _context.Products
        //        .SingleOrDefaultAsync(m => m.Id == id);

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        //// POST: Products/Delete/5z
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var product = await _context.Products.SingleOrDefaultAsync(m => m.Id == id);
        //    _context.Products.Remove(product);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        ////Displays items needed to be restocked
        //[Route("Products/DisplayRestock")]
        //public IActionResult DisplayRestock()
        //{
        //    this.ViewBag.Active = false;
        //    List<RestockProducts> list = RestockList();
        //    return this.View("DisplayRestock", list);
        //}


        //private bool ProductExists(int id)
        //{
        //    return _context.Products.Any(e => e.Id == id);
        //}

        ////Writes items needed to be restocked to a Queue as well as a Redis Cache
        //public void Restock(Product product)
        //{
            
        //    var restockQueue = this.queueClient.GetQueueReference("<name of queue>"); 
        //    var queueMsg = new CloudQueueMessage(product.Name + " : " + product.Description);
        //    string restock = product.Name + "," + product.Description;
        //    string id = product.Id.ToString();
        //    List<String> temp = new List<String>();


        //    if ((id != "" || restock != null) && product.Quantity == 0)
        //    {
        //        //writes to Queue
        //        restockQueue.AddMessageAsync(queueMsg);
        //        //writes to Cache
        //        this.cache.SetString(id, restock);

        //        temp.Add(id);
        //        keys = temp;
  
        //    }
        //}

        //// Reads the products that are currently in the cache and adds them to a list 
        //// GET: Products/Restock
        //[ValidateAntiForgeryToken]
        //public List<RestockProducts> RestockList()
        //{
        //    List<RestockProducts> items = new List<RestockProducts>();

        //    while (keys != null)
        //    {
        //        if (keys.Count > 0)
        //        {
        //            foreach (string key in keys)
        //            {
        //                string item = this.cache.GetString(key);
        //                List<String> substrings = item.Split(",").ToList();
        //                string name = substrings[0];
        //                string description = substrings[1];
        //                RestockProducts rp = new RestockProducts();
        //                rp.Id = Convert.ToInt32(key);
        //                rp.Name = name;
        //                rp.Description = description;
        //                items.Add(rp);
        //            }
        //        }

        //        return items;
        //    }
        //    return items;
        //}

    }
}
