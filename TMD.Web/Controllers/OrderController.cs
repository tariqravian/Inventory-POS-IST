﻿using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using TMD.Interfaces.IServices;
using TMD.Models.DomainModels;
using TMD.Models.RequestModels;
using TMD.Models.ResponseModels;
using TMD.Web.ModelMappers;
using TMD.Web.Models;
using TMD.Web.ViewModels;
using TMD.Web.ViewModels.Common;
using System.Threading.Tasks;
namespace TMD.Web.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class OrderController : BaseController
    {
        private readonly IOrdersService orderService;
        private readonly IProductService productService;
        private readonly IOrderItemService orderItemService;
        private readonly IProductConfigurationService configurationService;

        public ActionResult Index()
        {
            //if (Request.UrlReferrer == null || Request.UrlReferrer.AbsolutePath == "/Orders/EbayItemImportLV")
            //{
            //    Session["PageMetaData"] = null;
            //}

            OrderSearchRequest viewModel = Session["PageMetaData"] as OrderSearchRequest;

            Session["PageMetaData"] = null;
            ViewBag.MessageVM = TempData["message"] as MessageViewModel;
            var toReturnModel = new OrderViewModel
            {
                SearchRequest = viewModel ?? new OrderSearchRequest()
            };
            
            return View(toReturnModel);
        }
        [HttpPost]
        public ActionResult Index(OrderSearchRequest oRequest)
        {
            OrderSearchResponse oResponse = orderService.GetOrdersSearchResponse(oRequest);
            List<OrderListViewModel> oList = oResponse.Orders.Select(x => x.CreateFromServerToLVClient()).ToList();
            OrderViewModel oVModel = new OrderViewModel();
            oVModel.data = oList;
            oVModel.recordsTotal = oResponse.TotalCount;
            oVModel.recordsFiltered = oResponse.FilteredCount;



            Session["PageMetaData"] = oRequest;
            var toReturn = Json(oVModel, JsonRequestBehavior.AllowGet);
            return toReturn;
        }

        public OrderController(IOrdersService orderService, IProductService productService, IOrderItemService orderItemService, IProductConfigurationService configurationService)
        {
            this.orderService = orderService;
            this.productService = productService;
            this.orderItemService = orderItemService;
            this.configurationService = configurationService;
        }

        // GET: ProductCategory/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductCategory/Create
        public ActionResult Create(long ? id)
        {
            OrderModel toSend = new OrderModel();
            if (id == null || id == 0)
                toSend.OrderItems = new List<OrderItemModel>();
            else
            {
                //Means Edit case
                toSend = orderService.GetOrders(id.Value).CreateFromServerToClient();
            }

            return View(toSend);
        }

        // POST: ProductCategory/Create
        [HttpPost]
        public ActionResult Create(OrderModel orderDetail)
        {
            try
            {
                SetUserInfo(orderDetail);
                string email = GetConfigEmail();
                // TODO: Add insert logic here
                if (orderDetail.OrderId <= 0)
                {

                    var order = orderDetail.CreateFromClientToServer();
                   
                    orderService.AddService(order);
                    orderDetail.OrderId = order.OrderId;
                    new Task(() => { SendEmail(order, email); }).Start();
                }
                else
                {
                    var order = orderDetail.CreateFromClientToServer();
                    
                    orderService.UpdateService(order);
                    orderItemService.AddUpdateService(order);
                    new Task(() => { SendEmail(order, email); }).Start();
                }
                
            //    new Task(() => { Foo2(42, "life, the universe, and everything"); }).Start();
                return View();

            }
            catch
            {
                return View();
            }
        }

        private bool SendEmail(Order order,string email)
        {
            if (string.IsNullOrEmpty(email) || email.ToLower() == "none")
            {
                return false;
            }
            string subject = "";
            if (order.IsModified)
            {
                subject = "Modified: Order: " + order.OrderId;
            }
            else
            {
                subject = "Created: Order: " + order.OrderId;
            }
            string body = "Total Sale: " + order.OrderItems.Sum(x => x.SalePrice);
            body += " Total Discount: " + order.OrderItems.Sum(x => x.Discount);
            body += " Total Qty: " + order.OrderItems.Sum(x => x.Quantity);
            if (order.IsModified)
            {
                //Just enter that order was modified
                body = "Order Modified at: " + DateTime.Now.ToShortDateString() + " By:" + User.Identity.Name;
            }
            Utility.SendEmailAsync(email,subject,body);
            return true;
            //Utility.SendEmailAsync(email,"");
        }
        private void SetUserInfo(OrderModel orderDetail)
        {
               string name = User.Identity.Name;
            if (orderDetail.OrderId <= 0)
            {
                orderDetail.RecCreatedDate = orderDetail.RecLastUpdatedDate = DateTime.Now;

                orderDetail.RecCreatedBy = orderDetail.RecLastUpdatedBy = name;

            }
            else
            {
                orderDetail.RecLastUpdatedDate = DateTime.Now;
                orderDetail.RecLastUpdatedBy = User.Identity.Name;

            }

            List<OrderItemModel> NotUpdatedList = new List<OrderItemModel>();

            foreach (var item in orderDetail.OrderItems)
            {
                if (item.OrderItemId <= 0)
                {
                    item.RecCreatedDate = item.RecLastUpdatedDate = DateTime.Now;
                    item.RecCreatedBy = item.RecLastUpdatedBy = User.Identity.Name;
                    //GetSalePrice and set it
                    var product = productService.GetProduct(item.ProductId);
                    item.MinSalePriceAllowed = product.MinSalePriceAllowed;
                    item.PurchasePrice = product.PurchasePrice;
                    item.SalePrice = product.SalePrice;
                    if(orderDetail.OrderId>0)
                        orderDetail.IsModified = true;//Means a previous order had a new entery. I know this because orderid >0 and orderitem id<=0
                }
                else
                {
                    if (item.IsModified)
                    {
                        item.RecLastUpdatedBy = name;
                        item.RecLastUpdatedDate = DateTime.Now;
                        //FETCH FROM DB AND SET THE VALUES
                        var orderItem = orderItemService.GetOrderItemById(item.OrderItemId);
                        item.SalePrice = orderItem.SalePrice;
                        item.PurchasePrice = orderItem.PurchasePrice;
                        item.MinSalePriceAllowed = orderItem.MinSalePriceAllowed;
                        item.RecCreatedBy = orderItem.RecCreatedBy;
                        item.RecCreatedDate = orderItem.RecCreatedDate;
                        item.OrderId = orderDetail.OrderId;
                        orderDetail.IsModified = true;//means there is some modification in order. Only qty and Discount can be updated
                    }
                    else
                    {
                        NotUpdatedList .Add(item);
                    }
                }
            }
            foreach (var orderItemModel in NotUpdatedList)
            {
                orderDetail.OrderItems.Remove(orderItemModel);
            }

        }

        // GET: ProductCategory/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductCategory/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductCategory/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductCategory/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public string GetConfigEmail()
        {
            if (Session[Utility.ConfigEmail] == null)
            {
                var config = configurationService.GetDefaultConfiguration();
                var email = config.Emails;
                if (string.IsNullOrEmpty(email))
                    Session[Utility.ConfigEmail] = "NONE";
                else
                    Session[Utility.ConfigEmail] = email;
                return email;
                
            }
            else
            {

                return Session[Utility.ConfigEmail].ToString();
            }
        }
    }
}
