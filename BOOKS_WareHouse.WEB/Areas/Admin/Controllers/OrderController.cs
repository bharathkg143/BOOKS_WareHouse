using BOOKS_WareHouse.DataAccess.Data;
using BOOKS_WareHouse.DataAccess.Repository.IRepository;
using BOOKS_WareHouse.Models;
using Microsoft.AspNetCore.Mvc;

namespace BOOKS_WareHouse.WEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;

        public OrderController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
           
            List<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(null,includeProperties:"applicationUser").ToList();
            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
