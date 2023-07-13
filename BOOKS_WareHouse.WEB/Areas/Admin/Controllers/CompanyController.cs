using BOOKS_WareHouse.DataAccess.Repository.IRepository;
using BOOKS_WareHouse.Models;
using BOOKS_WareHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BOOKS_WareHouse.WEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> companies = _unitOfWork.Company.GetAll().ToList();
            return View(companies);
        }

        public IActionResult Upsert(int? id)
        {
            if(id == null || id==0)
            {
                Company company = new Company();
                company.Id = 0;
                return View(company);
            }
            else
            {
                Company company = _unitOfWork.Company.Get(x => x.Id == id);
                return View(company);
            }
            
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    TempData["Success"] = "Company created successfully";
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    TempData["Success"] = "Company updated successfully";
                    _unitOfWork.Company.Update(company);
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
            {
                return BadRequest();
            }
            Company? company = _unitOfWork.Company.Get(x => x.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        [HttpPost,ActionName("Delete")]
        public IActionResult DeleteCompany(int id)
        {
            Company? company = _unitOfWork.Company.Get(x => x.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            _unitOfWork.Company.Remove(company);
            _unitOfWork.Save();
            TempData["Success"] = "Company deleted successfully";

            return RedirectToAction("Index");
        }
    }
}
