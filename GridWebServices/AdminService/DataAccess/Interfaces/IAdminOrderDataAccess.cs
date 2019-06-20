using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.DataAccess.Interfaces
{
    public interface IAdminOrderDataAccess
    {
        Task<List<AdminService.Models.OrderList>> GetOrdersList(int? deliveryStatus, DateTime? fromDate, DateTime? toDate);
    }
}
