using BOOKS_WareHouse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOOKS_WareHouse.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id,string orderStatus,string? paymentStatus=null);
        void UpdatePaymentStripeId(int id, string sessionId, string paymentIntentId);
    }
}
