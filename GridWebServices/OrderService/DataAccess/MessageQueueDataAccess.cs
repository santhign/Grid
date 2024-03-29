﻿using OrderService.Models;
using Core.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Core.Enums;
using InfrastructureService;
using Core.Models;
using Core.Extensions;
using Newtonsoft.Json;

namespace OrderService.DataAccess
{
    public class MessageQueueDataAccess : IMessageQueueDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;
        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public MessageQueueDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the message body by change request.
        /// </summary>
        /// <param name="changeRequestId">The change request identifier.</param>
        /// <returns></returns>
        public async Task<MessageBodyForCR> GetMessageBodyByChangeRequest(int changeRequestId)
        {
            try
            {

                DataSet ds = new DataSet();
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int ),
                     };

                parameters[0].Value = changeRequestId;

                _DataHelper = new DataAccessHelper(DbObjectNames.CR_GetMessageBody, parameters, _configuration);


                var result = await _DataHelper.RunAsync(ds);


                var msgBody = new MessageBodyForCR();

                if (ds.Tables.Count > 0)
                {
                    msgBody = (from model in ds.Tables[0].AsEnumerable()
                               select new MessageBodyForCR()
                               {
                                   ChangeRequestID = model.Field<int>("ChangeRequestID"),
                                   AccountID = model.Field<int>("AccountID"),
                                   CustomerID = model.Field<int>("CustomerID"),
                                   SubscriberID = model.Field<int?>("SubscriberID"),
                                   OrderNumber = model.Field<string>("OrderNumber"),
                                   RequestOn = model.Field<DateTime>("RequestOn"),
                                   EffectiveDate = model.Field<DateTime?>("EffectiveDate"),
                                   BillingUnit = model.Field<string>("BillingUnit"),
                                   BillingFloor = model.Field<string>("BillingFloor"),
                                   BillingBuildingNumber = model.Field<string>("BillingBuildingNumber"),
                                   BillingBuildingName = model.Field<string>("BillingBuildingName"),
                                   BillingStreetName = model.Field<string>("BillingStreetName"),
                                   BillingPostCode = model.Field<string>("BillingPostCode"),
                                   BillingContactNumber = model.Field<string>("BillingContactNumber"),
                                   MobileNumber = model.Field<string>("MobileNumber"),
                                   PremiumType = model.Field<int?>("PremiumType"),
                                   IsPorted = model.Field<int?>("IsPorted"),
                                   IsOwnNumber = model.Field<int?>("IsOwnNumber"),
                                   DonorProvider = model.Field<string>("DonorProvider"),
                                   PortedNumberTransferForm = model.Field<string>("PortedNumberTransferForm"),
                                   PortedNumberOwnedBy = model.Field<string>("PortedNumberOwnedBy"),
                                   PortedNumberOwnerRegistrationID = model.Field<string>("PortedNumberOwnerRegistrationID"),
                                   Title = model.Field<string>("Title"),
                                   Name = model.Field<string>("Name"),
                                   Email = model.Field<string>("Email"),
                                   Nationality = model.Field<string>("Nationality"),
                                   IdType = model.Field<string>("IDType"),
                                   IdNumber = model.Field<string>("IDNumber"),
                                   IsSameAsBilling = model.Field<int?>("IsSameAsBilling"),
                                   ShippingUnit = model.Field<string>("ShippingUnit"),
                                   ShippingFloor = model.Field<string>("ShippingFloor"),
                                   ShippingBuildingNumber = model.Field<string>("ShippingBuildingNumber"),
                                   ShippingBuildingName = model.Field<string>("ShippingBuildingName"),
                                   ShippingStreetName = model.Field<string>("ShippingStreetName"),
                                   ShippingPostCode = model.Field<string>("ShippingPostCode"),
                                   ShippingContactNumber = model.Field<string>("ShippingContactNumber"),
                                   AlternateRecipientContact = model.Field<string>("AlternateRecipientContact"),
                                   AlternateRecipientName = model.Field<string>("AlternateRecipientName"),
                                   AlternateRecipientEmail = model.Field<string>("AlternateRecipientEmail"),
                                   PortalSlotID = model.Field<string>("PortalSlotID"),
                                   SlotDate = model.Field<DateTime?>("SlotDate"),
                                   SlotFromTime = model.Field<TimeSpan?>("SlotFromTime"),
                                   SlotToTime = model.Field<TimeSpan?>("SlotToTime"),
                                   ScheduledDate = model.Field<DateTime?>("ScheduledDate"),
                                   OldMobileNumber = model.Field<string>("OldMobileNumber"),
                                   NewMobileNumber = model.Field<string>("NewMobileNumber"),
                                   OldSIM = model.Field<string>("OldSIM"),
                                   ServiceFee = model.Field<double?>("ServiceFee"),
                                   AmountPaid = model.Field<double?>("AmountPaid"),
                                   PaymentMode = model.Field<string>("PaymentMode"),
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
                                   InvoiceUrl = model.Field<string>("InvoiceUrl"),
                                   ProcessedOn = model.Field<DateTime?>("ProcessedOn"),
                                   InvoiceNumber = model.Field<string>("InvoiceNumber"),
                               }).FirstOrDefault();

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count != 0)
                    {
                        msgBody.Bundles = (from model in ds.Tables[1].AsEnumerable()
                                           select new BundleDetails()
                                           {
                                               BundleID = model.Field<int?>("BundleID"),
                                               BSSPlanCode = model.Field<string>("BSSPlanCode"),
                                               BSSPlanName = model.Field<string>("BSSPlanName"),
                                               PlanType = model.Field<int?>("PlanType"),
                                               OldBundleID = model.Field<int?>("OldBundleID"),
                                               PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                               PortalDescription = model.Field<string>("PortalDescription"),
                                               TotalData = model.Field<double?>("TotalData"),
                                               TotalSMS = model.Field<double?>("TotalSMS"),
                                               TotalVoice = model.Field<double?>("TotalVoice"),
                                               ApplicableSubscriptionFee = model.Field<double?>("ApplicableSubscriptionFee"),
                                               ServiceName = model.Field<string>("ServiceName"),
                                               ApplicableServiceFee = model.Field<double?>("ApplicableServiceFee"),
                                               OldPlanID = model.Field<int?>("OldPlanID"),
                                               OldBSSPlanId = model.Field<string>("OldBSSPlanId"),
                                               OldBSSPlanName = model.Field<string>("OldBSSPlanName"),

                                           }).ToList();

                        msgBody.CurrBundles = (from model in ds.Tables[2].AsEnumerable()
                                               select new CurrBundleDetails()
                                               {
                                                   BundleID = model.Field<int?>("BundleID"),
                                                   BSSPlanCode = model.Field<string>("BSSPlanCode"),
                                                   BSSPlanName = model.Field<string>("BSSPlanName"),
                                                   PlanType = model.Field<int?>("PlanType"),
                                                   StartDate = model.Field<DateTime?>("StartDate"),
                                                   ExpiryDate = model.Field<DateTime?>("ExpiryDate"),

                                               }).ToList();


                    }
                    if (ds.Tables.Count > 2 && ds.Tables[3].Rows.Count != 0)
                        msgBody.Charges = (from model in ds.Tables[3].AsEnumerable()
                                           select new ChargesDetails()
                                           {
                                               ChangeRequestID = model.Field<int>("ChangeRequestID"),
                                               SubscriberID = model.Field<int?>("SubscriberID"),
                                               PortalServiceName = model.Field<string>("PortalServiceName"),
                                               ServiceFee = model.Field<double?>("ServiceFee"),
                                               IsRecurring = model.Field<int?>("IsRecurring"),
                                               IsGSTIncluded = model.Field<int?>("IsGSTIncluded"),


                                           }).ToList();
                }

                return msgBody;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }


        public async Task<MessageDetailsForCROrOrder> GetMessageDetails(string MPGSOrderID)
        {
            try
            {
                DataTable dt = new DataTable();

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@MPGSOrderID",  SqlDbType.NVarChar )
            };

                parameters[0].Value = MPGSOrderID;

                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_GetCROrOrderDetailsForMessageQueue, parameters, _configuration);


                var result = await _DataHelper.RunAsync(dt);

                //if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                //    return new DatabaseResponse { ResponseCode = result };

                var msgDetails = new MessageDetailsForCROrOrder();

                if (dt.Rows.Count > 0)
                {
                    msgDetails = (from model in dt.AsEnumerable()
                                  select new MessageDetailsForCROrOrder()
                                  {
                                      ChangeRequestID = model.Field<int>("ChangeRequestID"),
                                      RequestTypeID = model.Field<int>("RequestTypeID")

                                  }).FirstOrDefault();
                }


                return msgDetails;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<string> PublishMessageToMessageQueue(string topicName, object msgBody, Dictionary<string, string> messageAttribute, string subject)
        {
            try
            {
                var publisher = new InfrastructureService.MessageQueue.Publisher(_configuration, topicName);
                return await publisher.PublishAsync(msgBody, messageAttribute, subject);

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                throw;
            }

        }

        public async Task<string> PublishMessageToMessageQueue(string topicName, object msgBody, Dictionary<string, string> messageAttribute)
        {
            try
            {
                var publisher = new InfrastructureService.MessageQueue.Publisher(_configuration, topicName);
                return await publisher.PublishAsync(msgBody, messageAttribute);

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                throw;
            }

        }

        public async Task<int> InsertMessageInMessageQueueRequest(MessageQueueRequest messageQueueRequest)
        {
            try
            {
                SqlParameter[] parameters =
            {
                    new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                    new SqlParameter( "@SNSTopic",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MessageAttribute",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MessageBody",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Status",  SqlDbType.Int ),
                    new SqlParameter( "@PublishedOn",  SqlDbType.DateTime ),
                    new SqlParameter( "@CreatedOn",  SqlDbType.DateTime ),
                    new SqlParameter( "@NumberOfRetries",  SqlDbType.Int ),
                    new SqlParameter( "@LastTriedOn",  SqlDbType.DateTime)
            };

                parameters[0].Value = messageQueueRequest.Source;
                parameters[1].Value = messageQueueRequest.SNSTopic;
                parameters[2].Value = messageQueueRequest.MessageAttribute;
                parameters[3].Value = messageQueueRequest.MessageBody;
                parameters[4].Value = messageQueueRequest.Status;
                parameters[5].Value = messageQueueRequest.PublishedOn;
                parameters[6].Value = messageQueueRequest.CreatedOn;
                parameters[7].Value = messageQueueRequest.NumberOfRetries;
                parameters[8].Value = messageQueueRequest.LastTriedOn;


                _DataHelper = new DataAccessHelper(DbObjectNames.z_InsertIntoMessageQueueRequests, parameters, _configuration);


                return await _DataHelper.RunAsync();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetOrderMessageQueueBody(int orderId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@OrderID",  SqlDbType.Int )

                };

                parameters[0].Value = orderId;

                _DataHelper = new DataAccessHelper("Orders_GetMessageQueueBody", parameters, _configuration);

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
                                                  SubscriberID = model.Field<int?>("SubscriberID"),
                                                  AdminServiceID = model.Field<int?>("AdminServiceID"),
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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task InsertMessageInMessageQueueRequestException(MessageQueueRequestException messageQueueRequestException)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                    new SqlParameter( "@SNSTopic",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MessageAttribute",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MessageBody",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Status",  SqlDbType.Int ),
                    new SqlParameter( "@PublishedOn",  SqlDbType.DateTime ),
                    new SqlParameter( "@CreatedOn",  SqlDbType.DateTime ),
                    new SqlParameter( "@NumberOfRetries",  SqlDbType.Int ),
                    new SqlParameter( "@LastTriedOn",  SqlDbType.DateTime),
                    new SqlParameter( "@Remark",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Exception",  SqlDbType.NVarChar )

            };

                parameters[0].Value = messageQueueRequestException.Source;
                parameters[1].Value = messageQueueRequestException.SNSTopic;
                parameters[2].Value = messageQueueRequestException.MessageAttribute;
                parameters[3].Value = messageQueueRequestException.MessageBody;
                parameters[4].Value = messageQueueRequestException.Status;
                parameters[5].Value = messageQueueRequestException.PublishedOn;
                parameters[6].Value = messageQueueRequestException.CreatedOn;
                parameters[7].Value = messageQueueRequestException.NumberOfRetries;
                parameters[8].Value = messageQueueRequestException.LastTriedOn;
                parameters[9].Value = messageQueueRequestException.Remark;
                parameters[10].Value = messageQueueRequestException.Exception;


                _DataHelper = new DataAccessHelper(DbObjectNames.z_UpdateStatusInMessageQueueRequestsException, parameters, _configuration);


                await _DataHelper.RunAsync();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        /// <summary>
        /// Gets the message body by change request.
        /// </summary>
        /// <param name="CustomerID">The change request identifier.</param>
        /// <returns></returns>
        public async Task<ProfileMQ> GetProfileUpdateMessageBody(int CustomerID)
        {
            try
            {

                DataSet ds = new DataSet();
                SqlParameter[] parameters =
                 {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                     };

                parameters[0].Value = CustomerID;

                _DataHelper = new DataAccessHelper("Customers_GetProfileMessageQueue", parameters, _configuration);


                var result = await _DataHelper.RunAsync(ds);


                var msgBody = new ProfileMQ();

                if (ds.Tables.Count > 0)
                {
                    msgBody = (from model in ds.Tables[0].AsEnumerable()
                               select new ProfileMQ()
                               {
                                   accountID = model.Field<int>("accountID"),
                                   customerID = model.Field<int>("customerID"),
                                   subscriberID = model.Field<int?>("subscriberID"),
                                   mobilenumber = model.Field<string>("mobilenumber"),
                                   MaskedCardNumber = model.Field<string>("MaskedCardNumber"),
                                   Token = model.Field<string>("Token"),
                                   CardType = model.Field<string>("CardType"),
                                   IsDefault = model.Field<int?>("IsDefault"),
                                   CardHolderName = model.Field<string>("CardHolderName"),
                                   ExpiryMonth = model.Field<int?>("ExpiryMonth"),
                                   ExpiryYear = model.Field<int?>("ExpiryYear"),
                                   CardFundMethod = model.Field<string>("CardFundMethod"),
                                   CardBrand = model.Field<string>("CardBrand"),
                                   CardIssuer = model.Field<string>("CardIssuer"),
                                   billingUnit = model.Field<string>("billingUnit"),
                                   billingFloor = model.Field<string>("billingFloor"),
                                   billingBuildingNumber = model.Field<string>("billingBuildingNumber"),
                                   billingBuildingName = model.Field<string>("billingBuildingName"),
                                   billingStreetName = model.Field<string>("billingStreetName"),
                                   billingPostCode = model.Field<string>("billingPostCode"),
                                   billingContactNumber = model.Field<string>("billingContactNumber"),
                                   email = model.Field<string>("email"),
                                   displayname = model.Field<string>("displayname"),
                                   paymentmode = model.Field<string>("paymentmode"),
                                   amountpaid = model.Field<double?>("amountpaid"),
                                   MPGSOrderID = model.Field<string>("MPGSOrderID"),
                                   invoicelist = model.Field<string>("invoicelist"),
                                   invoiceamounts = model.Field<string>("invoiceamounts")
                               }).FirstOrDefault();
                }

                return msgBody;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw;
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetAccountInvoiceMessageQueueBody(int invoiceId)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@InvoiceID",  SqlDbType.Int)
               };

                parameters[0].Value = invoiceId;

                _DataHelper = new DataAccessHelper("Orders_GetInvoiceMessageQueueBody", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    InvoceQM order = new InvoceQM();

                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {

                        order = (from model in ds.Tables[0].AsEnumerable()
                                 select new InvoceQM()
                                 {
                                     accountID = model.Field<int>("accountID"),
                                     customerID = model.Field<int>("customerID"),
                                     mobilenumber = model.Field<string>("mobilenumber"),
                                     email = model.Field<string>("email"),
                                     MPGSOrderID = model.Field<string>("MPGSOrderID"),
                                     MaskedCardNumber = model.Field<string>("MaskedCardNumber"),
                                     Token = model.Field<string>("Token"),
                                     CardType = model.Field<string>("CardType"),
                                     IsDefault = model.Field<int?>("IsDefault"),
                                     CardHolderName = model.Field<string>("CardHolderName"),
                                     ExpiryMonth = model.Field<int?>("ExpiryMonth"),
                                     ExpiryYear = model.Field<int?>("ExpiryYear"),
                                     CardFundMethod = model.Field<string>("CardFundMethod"),
                                     CardBrand = model.Field<string>("CardBrand"),
                                     CardIssuer = model.Field<string>("CardIssuer"),
                                     //paymentmode
                                     amountpaid = model.Field<double?>("amountPaid"),
                                     //invoicelist-- need to take from bss
                                     invoiceamounts = model.Field<double?>("invoiceamounts")
                                 }).FirstOrDefault();


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
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> GetRescheduleMessageQueueBody(int accountInvoiceID)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@AccountInvoiceID",  SqlDbType.Int )

                };

                parameters[0].Value = accountInvoiceID;

                _DataHelper = new DataAccessHelper("Orders_GetRescheduleMessageQueueBody", parameters, _configuration);

                DataSet ds = new DataSet();

                int result = await _DataHelper.RunAsync(ds); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    RescheduleDeliveryMessage rescheduleDelivery = new RescheduleDeliveryMessage();

                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {

                        rescheduleDelivery = (from model in ds.Tables[0].AsEnumerable()
                                              select new RescheduleDeliveryMessage()
                                              {
                                                  accountID = model.Field<int?>("accountID"),
                                                  customerID = model.Field<int?>("customerID"),
                                                  SourceType = model.Field<string>("SourceType"),
                                                  orderID = model.Field<int?>("orderID"),
                                                  orderNumber = model.Field<string>("orderNumber"),
                                                  orderDate = model.Field<DateTime?>("orderDate"),
                                                  name = model.Field<string>("name"),
                                                  email = model.Field<string>("email"),
                                                  nationality = model.Field<string>("nationality"),
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
                                                  CreatedOn = model.Field<DateTime?>("CreatedOn")

                                              }).FirstOrDefault();

                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            List<InvoiceCharges> Charges = new List<InvoiceCharges>();
                            if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                            {

                                Charges = (from model in ds.Tables[1].AsEnumerable()
                                           select new InvoiceCharges()
                                           {
                                               AccountInvoiceID = model.Field<int?>("AccountInvoiceID"),
                                               AdminServiceID = model.Field<int?>("AdminServiceID"),
                                               isGSTIncluded = model.Field<int?>("isGSTIncluded"),
                                               isRecurring = model.Field<int?>("isRecurring"),
                                               portalServiceName = model.Field<string>("portalServiceName"),
                                               serviceFee = model.Field<double?>("serviceFee"),
                                           }).ToList();

                            }
                            rescheduleDelivery.invoiceCharges = Charges;

                        }
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = rescheduleDelivery };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }

}
