﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EPaper.Data;
using EPaper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using EPaper.Helpers;

namespace EPaper.Models
{
    public class CartController : Controller
    {

        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                string userId = GetUserId();
                List<Cart> carts = _context.Carts
                    .Include(p => p.Product)
                    .Where(c => c.UserId == userId && c.OrderId == null)
                    .ToList();
                carts.Add(new Cart() { Quantity = 2 });
                if (carts != null)
                {
                    return View(carts);
                }
                else
                {
                    return View();
                }

            }
            else
            {
                List<Item> cart = new List<Item>();
                cart.Add(new Item { Quantity = 10 });
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
                cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
                ViewBag.cart = cart;
             /*   if (cart != null)
                {
                    ViewBag.total = cart.Sum(item => item.Product.Price * item.Quantity);
                } */
                cart.Add(new Item() { Quantity = 10 });
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddtoCart(int? id)
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
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddtoCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddtoCart([Bind("Id")] Product product)
        {

            if (User.Identity.IsAuthenticated)
            {

                string userId = GetUserId();
                Cart cart = ProductIsInCart(product.ProductId);

                if (cart != null)
                {
                    ++cart.Quantity;
                }
                else
                {
                    cart.UserId = userId;
                    cart.ProductId = product.ProductId;
                    cart.Quantity = 1;
                    _context.Carts.Add(cart);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                //Product product = new Product();
                if (SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart") == null)
                {
                    List<Item> cart = new List<Item>();
                    cart.Add(new Item { Product = _context.Products.Find(product.ProductId), Quantity = 1 });
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
                }
                else
                {
                    List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
                    int index = isExist(product.ProductId);
                    if (index != -1)
                    {
                        cart[index].Quantity++;
                    }
                    else
                    {
                        cart.Add(new Item { Product = _context.Products.Find(product.ProductId), Quantity = 1 });
                    }
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index", "Products");
        }

        // GET: Basket / Delete / 5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                string userId = GetUserId();
                var product = ProductIsInCart(id);

                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            return View();
        }

        // POST: Basket/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            if (User.Identity.IsAuthenticated)
            {
                string userId = GetUserId();
                var product = ProductIsInCart(id);

                if (product == null)
                {
                    return NotFound();
                }

                if (product.Quantity <= 1)
                {
                    _context.Carts.Remove(product);
                }
                else
                {
                    product.Quantity = --product.Quantity;
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }

        // GET : /CheckOut
        public IActionResult CheckOut()
        {
            if (User.Identity.IsAuthenticated)


            {
                return RedirectToAction("Create", "Payment");

            }
            return NotFound();
        }

        private string GetUserId()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return userId;
        }

        private Cart ProductIsInCart(int? id)
        {
            string userId = GetUserId();
            var product = _context.Carts
              .FirstOrDefault(i => i.ProductId == id
                                && i.UserId == userId
                                && i.OrderId == null);
            return product;
        }
        private int isExist(int id)
        {
            List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].Product.ProductId.Equals(id))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
