using BOOKS_WareHouse.DataAccess.Data;
using BOOKS_WareHouse.DataAccess.Repository.IRepository;
using BOOKS_WareHouse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOOKS_WareHouse.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Company company)
        {
            Company? companyToUpdate = _context.Companies.FirstOrDefault(x => x.Id==company.Id);
            if (companyToUpdate != null)
            {
                companyToUpdate.Name=company.Name;
                companyToUpdate.StreetAddress=company.StreetAddress;
                companyToUpdate.City=company.City;
                companyToUpdate.PostalCode=company.PostalCode;
                companyToUpdate.PhoneNumber=company.PhoneNumber;
            }
        }
    }
}
