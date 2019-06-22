using AdminService.Models;
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

        Task<AdminService.Models.OrderDetails> GetOrderDetails(int orderID);

        Task<DatabaseResponse> UpdateNRICDetails(int adminUserId, int verificationStatus, NRICDetailsRequest request);

        Task<DatabaseResponse> GetEmailNotificationTemplate(string templateName);

        Task<DatabaseResponse> CreateEMailNotificationLogForDevPurpose(NotificationLogForDevPurpose log);
        Task<DatabaseResponse> GetConfiguration(string configType);

        Task<DatabaseResponse> CreateTokenForVerificationRequests(int orderId);
    }
}
