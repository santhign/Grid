﻿using AdminService.Models;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.DataAccess.Interfaces
{
    public interface IAdminOrderDataAccess
    {
        Task<List<OrderList>> GetOrdersList(int? deliveryStatus, DateTime? fromDate, DateTime? toDate); 
        Task<DatabaseResponse> GetEmailNotificationTemplate(string templateName);

        Task<DatabaseResponse> CreateEMailNotificationLogForDevPurpose(NotificationLogForDevPurpose log);
        Task<DatabaseResponse> GetConfiguration(string configType);

        Task<DatabaseResponse> CreateTokenForVerificationRequests(int orderId);

        Task<List<IDVerificaionHistory>> GetNRICOrderDetailsHistory(int orderID);
    }
}
