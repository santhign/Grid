using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Core.Helpers;
using Core.Models;
using Core.Extensions;
using System.Linq;

namespace Core.DataAccess
{
    public class BuddyDataAccess
    {

        internal DataAccessHelper _DataHelper = null;
       
        public async Task<DatabaseResponse> GetPendingBuddyList(string connectionString)
        {
            try
            { 
                _DataHelper = new DataAccessHelper("Order_GetPendingBuddyOrders", connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 109 /105

                DatabaseResponse response = new DatabaseResponse();

                List<PendingBuddy> pendingBuddies = new List<PendingBuddy>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    pendingBuddies = (from model in dt.AsEnumerable()
                                select new PendingBuddy()
                                {
                                     PendingBuddyID = model.Field<int>("PendingBuddyID"),
                                     OrderID = model.Field<int>("OrderID"),
                                     PendingBuddyOrderListID = model.Field<int>("PendingBuddyOrderListID"),
                                     OrderSubscriberID = model.Field<string>("OrderSubscriberID"),                                  
                                     MobileNumber  = model.Field<string>("MobileNumber"),
                                     DateCreated = model.Field<string>("DateCreated"),
                                     IsProcessed = model.Field<bool>("IsProcessed")                                      
                                }).ToList();                   

                    response = new DatabaseResponse { ResponseCode = result, Results = pendingBuddies };
                }


                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }
}
