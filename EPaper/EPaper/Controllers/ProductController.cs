﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPaper.Data;
using EPaper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace EPaper.Models
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        


        public ProductController(ApplicationDbContext context)
        {
            _context = context;
          
        }

        public IActionResult Index()
        {
            return View();
        }

        //////////////////////////ADMIN////////////////////////
        // GET: Product/Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("Product/Admin")]
        public async Task<IActionResult> AdminIndex()
        {

            var applicationDbContext = _context.Products;
                return View(await applicationDbContext.ToListAsync());
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        /// <summary>
        ///  POST:/ Delete/Product/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("AdminIndex");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
       
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            string type = product.Type;
            switch (type)
            {
                case "Book":
                    return RedirectToAction("Edit","Book",product);
                case "Magazine":
                    return RedirectToAction("Edit", "Magazine",product);
                case "Cd":
                    return RedirectToAction("Edit", "Cd",product);
                default:
                    return RedirectToAction("AdminIndex");

            }

            
        }
    }
}