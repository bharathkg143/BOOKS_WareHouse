﻿using BOOKS_WareHouse.DataAccess.Repository.IRepository;
using BOOKS_WareHouse.Models;
using BOOKS_WareHouse.Utility;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using System.Security.Claims;
using System.Security.Policy;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;

namespace BOOKS_WareHouse.WEB.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<ShoppingCart> shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList();

            var OrderTotal = 0.0;
            foreach (var cart in shoppingCartList)
            {
                double price = GetPriceBasedOnQuantity(cart);

                OrderTotal += (price * cart.Count);

            }
            ViewBag.OrderTotal = OrderTotal;
            return View(shoppingCartList);
        }

        public IActionResult Plus(int cartId)
        {
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId);
            shoppingCart.Count += 1;
            _unitOfWork.ShoppingCart.Update(shoppingCart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId,tracked:true);
            if (shoppingCart.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.
                    GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).Count()-1);

                _unitOfWork.ShoppingCart.Remove(shoppingCart);   
            }
            else
            {
                shoppingCart.Count -= 1;
                _unitOfWork.ShoppingCart.Update(shoppingCart);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int cartId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId);

            _unitOfWork.ShoppingCart.Remove(shoppingCart);
            _unitOfWork.Save();

            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.
                       GetAll(x => x.ApplicationUserId == userID).Count());
            TempData["Success"] = "Cart updated succefully";

            return RedirectToAction(nameof(Index));

        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<ShoppingCart> shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList();

            OrderHeader orderHeader = new OrderHeader();
            orderHeader.applicationUser = _unitOfWork.ApplicationUser.Get(x => x.Id == userId);

            orderHeader.Name = orderHeader.applicationUser.Name;
            orderHeader.PhoneNumber = orderHeader.applicationUser.PhoneNumber;
            orderHeader.StreetAddress = orderHeader.applicationUser.StreetAddress;
            orderHeader.PostalCode = orderHeader.applicationUser.PostalCode;
            orderHeader.City = orderHeader.applicationUser.City;
            orderHeader.State = orderHeader.applicationUser.State;

            var OrderTotal = 0.0;
            foreach (var cart in shoppingCartList)
            {
                double price = GetPriceBasedOnQuantity(cart);

                OrderTotal += (price * cart.Count);

            }
            ViewBag.OrderTotal = OrderTotal;
            ViewBag.ShoppingCartList = shoppingCartList;
            return View(orderHeader);
        }

        [HttpPost, ActionName("Summary")]
        public IActionResult SummaryPOST(OrderHeader orderHeader, OrderDetail orderDetail)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<ShoppingCart> shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList();

            orderHeader.OrderDate = System.DateTime.Now;
            orderHeader.ApplicationUserId = userId;
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(x => x.Id == userId);

            var OrderTotal = 0.0;
            foreach (var cart in shoppingCartList)
            {
                double price = GetPriceBasedOnQuantity(cart);
                orderHeader.OrderTotal += (price * cart.Count);
            }

            orderHeader.OrderStatus = SD.StatusPending;
            orderHeader.PaymentStatus = SD.PaymentStatusPending;

            _unitOfWork.OrderHeader.Add(orderHeader);
            _unitOfWork.Save();

            foreach (var cart in shoppingCartList)
            {
                orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    Count = cart.Count,
                    OrderHeaderId = orderHeader.Id,
                    Price = cart.Product.Price
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            //regular customer account we are capturing payment
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //stripe logic
                var domain = "https://localhost:7147/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/ShoppingCart/OrderConfirmation?id={orderHeader.Id}",
                    CancelUrl = domain + "Customer/ShoppingCart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach (var item in shoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Product.Price * 100),
                            Currency = "INR",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity=item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);

                _unitOfWork.OrderHeader.UpdatePaymentStripeId(orderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { orderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == id, includeProperties: "applicationUser");

            var service= new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdatePaymentStripeId(id, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }

            //Clearing Cart session after payment and order succefully confirm
            HttpContext.Session.Remove(SD.SessionCart);
            //HttpContext.Session.Clear();

            //To remove cart data after order success
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(x => x.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

        #region PriceCalcutatingMethod
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }

        #endregion

    }
}
