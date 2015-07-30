﻿// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------
#define TRACE
namespace CloudShop.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using CloudShop.Models;
    using System.Configuration;

    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Index()
        {
            ViewBag.Message = Environment.MachineName;


            System.Diagnostics.Trace.TraceError("HomeController Index()-TraceError trace output.");

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            int start = connectionString.IndexOf(";Password=");
            var end =connectionString.IndexOf(";",start+1);
            var connectionStringSafe = connectionString.Remove(start, end-start);
            ViewBag.ConnectionString = connectionStringSafe;

            string shopName = ConfigurationManager.AppSettings["CloudShopName"].ToString();

            //Alternative way to access variable through environment setting
            shopName= System.Environment.GetEnvironmentVariable("APPSETTING_CloudShopName");

            System.Diagnostics.Trace.TraceWarning("HomeController Index()- App Setting CloudShopName has value " + shopName + ".");
            ViewBag.ShopName = shopName;
            return Search(null);
        }

        [HttpPost]
        public ActionResult Add(string selectedProduct)
        {
            if (selectedProduct != null)
            {
                List<string> cart = this.Session["Cart"] as List<string> ?? new List<string>();
                cart.Add(selectedProduct);
                Session["Cart"] = cart;
                System.Diagnostics.Trace.TraceInformation("HomeController TraceInformation - Add()- product " + selectedProduct+ " added to cart");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Search(string SearchCriteria)
        {
            Services.IProductRepository productRepository = new Services.ProductsRepository();
            var products = string.IsNullOrEmpty(SearchCriteria) ?
                productRepository.GetProducts() : productRepository.Search(SearchCriteria);

            // add all products currently not in session
            var itemsInSession = this.Session["Cart"] as List<string> ?? new List<string>();
            var filteredProducts = products.Where(item => !itemsInSession.Contains(item));

            var model = new IndexViewModel()
            {
                Products = filteredProducts,
                SearchCriteria = SearchCriteria
            };

            return View("Index", model);
        }

        public ActionResult Checkout()
        {
            ViewBag.Message = Environment.MachineName;
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            int start = connectionString.IndexOf(";Password=");
            var end = connectionString.IndexOf(";", start + 1);
            var connectionStringSafe = connectionString.Remove(start, end - start);
            ViewBag.ConnectionString = connectionStringSafe;

            var itemsInSession = this.Session["Cart"] as List<string> ?? new List<string>();
            var model = new IndexViewModel()
            {
                Products = itemsInSession
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Remove(string Products)
        {
            if (Products != null)
            {
                var itemsInSession = this.Session["Cart"] as List<string>;
                if (itemsInSession != null)
                {
                    itemsInSession.Remove(Products);
                }
            }

            return RedirectToAction("Checkout");
        }
    }
}