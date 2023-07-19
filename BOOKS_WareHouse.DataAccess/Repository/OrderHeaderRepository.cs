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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
        private ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

		public void Update(OrderHeader orderHeader)
		{
			_context.OrderHeaders.Update(orderHeader);	
		}

        public void UpdatePaymentStripeId(int id, string sessionId, string paymentIntentId)
        {
            var orderHeaderFromDB = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            
            if(orderHeaderFromDB != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    orderHeaderFromDB.SessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    orderHeaderFromDB.PaymentIntentId = paymentIntentId;
                    orderHeaderFromDB.PaymentDate= DateTime.Now;
                }
            }
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderHeaderFromDB = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderHeaderFromDB != null)
            {
                orderHeaderFromDB.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderHeaderFromDB.PaymentStatus= paymentStatus;
                }
            }
        }
    }
}
