﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficCaseApp.Models;
using TrafficCaseApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace TrafficCaseApp.Controllers
{
    [Authorize]
    public class CasesController : Controller
    {
        private ITrafficCaseRepository caseRepository;
        private IQueueClient queueClient;

        public CasesController(ITrafficCaseRepository caseRepository, IQueueClient queueClient)
        {
            this.caseRepository = caseRepository;
            this.queueClient = queueClient;
        }

        // GET: Cases
        public IActionResult Index()
        {
            var cases = this.caseRepository.GetCases();
            return View(cases);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            CaseViewModel caseVM = new CaseViewModel();
            TrafficCase trafficCase = new TrafficCase();
            caseVM.Case = trafficCase;
            caseVM.Statuses = this.caseRepository.GetStatuses().ToSelectList();
            return View(caseVM);
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "Case")]TrafficCase trafficCase)
        {
            if (ModelState.IsValid)
            {
                trafficCase.Id = Guid.NewGuid();
                await this.caseRepository.CreateCase(trafficCase);
                await this.queueClient.AddCaseToQueue(trafficCase);
                return RedirectToAction(nameof(Index));
            }
            CaseViewModel caseVM = new CaseViewModel();
            caseVM.Case = trafficCase;
            caseVM.Statuses = this.caseRepository.GetStatuses().ToSelectList();
            return View(caseVM);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trafficCase = await this.caseRepository.GetCase(id);
            if (trafficCase == null)
            {
                return NotFound();
            }
            CaseViewModel caseVM = new CaseViewModel();
            caseVM.Case = trafficCase;
            caseVM.Statuses = this.caseRepository.GetStatuses().ToSelectList();
            return View(caseVM);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind(Prefix = "Case")]TrafficCase trafficCase)
        {
            CaseViewModel caseVM = new CaseViewModel();
            if (this.ModelState.IsValid)
            {
                caseVM.Case = trafficCase;
                caseVM.Statuses = this.caseRepository.GetStatuses().ToSelectList();
                return View(caseVM);
            }
            else
            {
                this.caseRepository.EditCase(trafficCase);
                this.queueClient.AddCaseToQueue(trafficCase);
                //  Restock(product);
                return this.RedirectToAction("Index");
            }
        }

        //// GET: Products/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caseDelete = await this.caseRepository.GetCase(id);

            if (caseDelete == null)
            {
                return NotFound();
            }

            return View(caseDelete);
        }

        //// POST: Products/Delete/5z
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await this.caseRepository.DeleteCase(id);
            return RedirectToAction(nameof(Index));
        }

        //Displays items needed to be restocked
        [Route("Cases/Closed")]
        public async Task<IActionResult> Closed()
        {
            this.ViewBag.Active = false;
            var list = await this.queueClient.GetClosedCases();
            return this.View("Closed", list);
        }


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
