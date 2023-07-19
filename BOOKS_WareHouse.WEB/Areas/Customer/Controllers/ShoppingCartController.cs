using BOOKS_WareHouse.DataAccess.Repository.IRepository;
using BOOKS_WareHouse.Models;
using BOOKS_WareHouse.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BOOKS_WareHouse.WEB.Areas.Customer.Controllers
{
    [Area("Customer")]
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
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId);
            if (shoppingCart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingCart);
                _unitOfWork.Save();
            }
            else
            {
                shoppingCart.Count -= 1;
                _unitOfWork.ShoppingCart.Update(shoppingCart);
                _unitOfWork.Save();

            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int cartId)
        {

            ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId);

            _unitOfWork.ShoppingCart.Remove(shoppingCart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Summary()
        {
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			List<ShoppingCart> shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList();

            OrderHeader orderHeader = new OrderHeader();
			orderHeader.applicationUser = _unitOfWork.ApplicationUser.Get(x=>x.Id==userId);

            orderHeader.Name=orderHeader.applicationUser.Name;
            orderHeader.PhoneNumber=orderHeader.applicationUser.PhoneNumber;
            orderHeader.StreetAddress=orderHeader.applicationUser.StreetAddress;
            orderHeader.PostalCode=orderHeader.applicationUser.PostalCode;
            orderHeader.City=orderHeader.applicationUser.City;
            orderHeader.State=orderHeader.applicationUser.State;

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

        [HttpPost,ActionName("Summary")]
        public IActionResult SummaryPOST(OrderHeader orderHeader,OrderDetail orderDetail)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<ShoppingCart> shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList();



            
            orderHeader.OrderDate=System.DateTime.Now;
            orderHeader.ApplicationUserId=userId;
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

            foreach(var cart in shoppingCartList)
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
            return RedirectToAction(nameof(OrderConfirmation),new {orderHeader.Id});
        }

        public IActionResult OrderConfirmation(int id)
        {
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
