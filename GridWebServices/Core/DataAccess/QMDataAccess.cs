using Core.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Core.Enums;
using Core.Models;
using Core.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace Core.DataAccess
{
    public class QMDataAccess
    {
        internal DataAccessHelper _DataHelper = null;
        public async Task<DatabaseResponse> GetOrderMessageQueueBody(int orderId, string connectionString)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Orders_GetMessageQueueBody", parameters, connectionString);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    OrderQM order = new OrderQM();

                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {

                        order = (from model in ds.Tables[0].AsEnumerable()
                                 select new OrderQM()
                                 {
                                     accountID = model.Field<int>("accountID"),
                                     customerID = model.Field<int>("customerID"),
                                     orderID = model.Field<int>("orderID"),
                                     orderNumber = model.Field<string>("orderNumber"),
                                     orderDate = model.Field<DateTime>("orderDate"),
                                     billingUnit = model.Field<string>("billingUnit"),
                                     billingFloor = model.Field<string>("billingFloor"),
                                     billingBuildingNumber = model.Field<string>("billingBuildingNumber"),
                                     billingBuildingName = model.Field<string>("billingBuildingName"),
                                     billingStreetName = model.Field<string>("billingStreetName"),
                                     billingPostCode = model.Field<string>("billingPostCode"),
                                     billingContactNumber = model.Field<string>("billingContactNumber"),
                                     orderReferralCode = model.Field<string>("orderReferralCode"),
                                     title = model.Field<string>("title"),
                                     name = model.Field<string>("name"),
                                     email = model.Field<string>("email"),
                                     nationality = model.Field<string>("nationality"),
                                     idType = model.Field<string>("idType"),
                                     idNumber = model.Field<string>("idNumber"),
                                     isSameAsBilling = model.Field<int?>("isSameAsBilling"),
                                     shippingUnit = model.Field<string>("shippingUnit"),
                                     shippingFloor = model.Field<string>("shippingFloor"),
                                     shippingBuildingNumber = model.Field<string>("shippingBuildingNumber"),
                                     shippingBuildingName = model.Field<string>("shippingBuildingName"),
                                     shippingStreetName = model.Field<string>("shippingStreetName"),
                                     shippingPostCode = model.Field<string>("shippingPostCode"),
                                     shippingContactNumber = model.Field<string>("shippingContactNumber"),
                                     alternateRecipientContact = model.Field<string>("alternateRecipientContact"),
                                     alternateRecipientName = model.Field<string>("alternateRecipientName"),
                                     alternateRecipientEmail = model.Field<string>("alternateRecipientEmail"),
                                     portalSlotID = model.Field<string>("portalSlotID"),
                                     slotDate = model.Field<DateTime?>("slotDate"),
                                     slotFromTime = model.Field<TimeSpan?>("slotFromTime"),
                                     slotToTime = model.Field<TimeSpan?>("slotToTime"),
                                     scheduledDate = model.Field<DateTime?>("scheduledDate"),
                                     submissionDate = model.Field<DateTime>("submissionDate"),
                                     serviceFee = model.Field<double?>("serviceFee"),
                                     amountPaid = model.Field<double?>("amountPaid"),
                                     paymentMode = model.Field<string>("paymentMode"),
                                     MPGSOrderID = model.Field<string>("MPGSOrderID"),
                                     MaskedCardNumber = model.Field<string>("MaskedCardNumber"),
                                     Token = model.Field<string>("Token"),
                                     CardType = model.Field<string>("CardType"),
                                     CardHolderName = model.Field<string>("CardHolderName"),
                                     ExpiryMonth = model.Field<int?>("ExpiryMonth"),
                                     ExpiryYear = model.Field<int?>("ExpiryYear"),
                                     CardFundMethod = model.Field<string>("CardFundMethod"),
                                     CardBrand = model.Field<string>("CardBrand"),
                                     CardIssuer = model.Field<string>("CardIssuer"),
                                     DateofBirth = model.Field<DateTime?>("DateofBirth"),
                                     ReferralCode = model.Field<string>("ReferralCode"),
                                     ProcessedOn = model.Field<DateTime?>("ProcessedOn"),
                                     InvoiceNumber = model.Field<string>("InvoiceNumber"),
                                     InvoiceUrl = model.Field<string>("InvoiceUrl"),
                                     CreatedOn = model.Field<DateTime>("CreatedOn")

                                 }).FirstOrDefault();

                        List<OrderSubscriber> Subscribers = new List<OrderSubscriber>();

                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            List<OrderSubscriptionQM> Bundles = new List<OrderSubscriptionQM>();

                            if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                            {

                                Bundles = (from model in ds.Tables[2].AsEnumerable()
                                           select new OrderSubscriptionQM()
                                           {
                                               SubscriberID = model.Field<int>("SubscriberID"),
                                               bundleID = model.Field<int>("bundleID"),
                                               bssPlanCode = model.Field<string>("bssPlanCode"),
                                               bssPlanName = model.Field<string>("bssPlanName"),
                                               planType = model.Field<int>("planType"),
                                               planMarketingName = model.Field<string>("planMarketingName"),
                                               portalDescription = model.Field<string>("portalDescription"),
                                               totalData = model.Field<double?>("totalData"),
                                               totalSMS = model.Field<double?>("totalSMS"),
                                               totalVoice = model.Field<double?>("totalVoice"),
                                               applicableSubscriptionFee = model.Field<double?>("applicableSubscriptionFee")
                                           }).ToList();

                            }



                            Subscribers = (from model in ds.Tables[1].AsEnumerable()
                                           select new OrderSubscriber()
                                           {
                                               OrderID = model.Field<int>("OrderID"),
                                               subscriberID = model.Field<int>("subscriberID"),
                                               mobileNumber = model.Field<string>("mobileNumber"),
                                               displayName = model.Field<string>("displayName"),
                                               isPrimaryNumber = model.Field<int>("isPrimaryNumber"),
                                               premiumType = model.Field<int?>("premiumType"),
                                               isPorted = model.Field<int?>("isPorted"),
                                               isOwnNumber = model.Field<int?>("isOwnNumber"),
                                               donorProvider = model.Field<string>("donorProvider"),
                                               DepositFee = model.Field<double?>("DepositFee"),
                                               IsBuddyLine = model.Field<int?>("IsBuddyLine"),
                                               LinkedSubscriberID = model.Field<int?>("LinkedSubscriberID"),
                                               RefOrderSubscriberID = model.Field<int?>("RefOrderSubscriberID"),
                                               portedNumberTransferForm = model.Field<string>("portedNumberTransferForm"),
                                               portedNumberOwnedBy = model.Field<string>("portedNumberOwnedBy"),
                                               portedNumberOwnerRegistrationID = model.Field<string>("portedNumberOwnerRegistrationID"),
                                               Bundles = Bundles != null ? Bundles.Where(b => b.SubscriberID == model.Field<int>("subscriberID")).ToList() : null
                                           }).ToList();

                            order.Subscribers = Subscribers;

                        }



                        List<OrderServiceCharge> serviceCharges = new List<OrderServiceCharge>();

                        if (ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
                        {

                            serviceCharges = (from model in ds.Tables[3].AsEnumerable()
                                              select new OrderServiceCharge()
                                              {
                                                  OrderID = model.Field<int>("OrderID"),
                                                  SubscriberID = model.Field<int>("SubscriberID"),
                                                  portalServiceName = model.Field<string>("portalServiceName"),
                                                  serviceFee = model.Field<double?>("serviceFee"),
                                                  isRecurring = model.Field<int>("isRecurring"),
                                                  isGSTIncluded = model.Field<int>("isGSTIncluded"),
                                              }).ToList();

                            order.Charges = serviceCharges;

                        }
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = order };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
              //  LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetCustomerOrderCount(int CustomerID, string connectionString)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int)
                };

                parameters[0].Value = CustomerID;

                _DataHelper = new DataAccessHelper("Orders_GetCustomerOrderCount", parameters, connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); //105/102

                DatabaseResponse response = new DatabaseResponse();

                int count = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    count = int.Parse(dt.Rows[0].ItemArray[0].ToString());
                }

                response = new DatabaseResponse { ResponseCode = result, Results = count };

                return response;
            }

            catch (Exception ex)
            {
               Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }
}
