using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BOOKS_WareHouse.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        //T-Category
        IEnumerable<T> GetAll(string? includeProperties = null);
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null);
        void Add(T entity);
        //void Update(T entity);//its complicated we update based on so many parameters so it should be difficult with this so keep separately in perticular classes repository
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
