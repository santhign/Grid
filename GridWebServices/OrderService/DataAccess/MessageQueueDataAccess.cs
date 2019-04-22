using OrderService.Models;
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
                                   OrderNumber = model.Field<string>("OrderNumber"),
                                   RequestOn = model.Field<DateTime>("RequestOn"),
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
                                   DonorProviderName = model.Field<string>("DonorProviderName"),
                                   PortedNumberTransferForm = model.Field<string>("PortedNumberTransferForm"),
                                   PortedNumberOwnedBy = model.Field<string>("PortedNumberOwnedBy"),
                                   PortedNumberOwnerRegistrationID = model.Field<string>("PortedNumberOwnerRegistrationID"),
                                   ReferralCode = model.Field<string>("ReferralCode"),
                                   Title = model.Field<string>("Title"),
                                   Name = model.Field<string>("Name"),
                                   Email = model.Field<string>("Email"),
                                   Nationality = model.Field<string>("Nationality"),
                                   IdentityCardType = model.Field<string>("IdentityCardType"),
                                   IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                   IsSameAsBilling = model.Field<string>("IsSameAsBilling"),
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
                                   SlotFromTime = model.Field<DateTime?>("SlotFromTime"),
                                   SlotToTime = model.Field<DateTime?>("SlotToTime"),
                                   ScheduledDate = model.Field<DateTime?>("ScheduledDate"),
                                   SubmissionDate = model.Field<DateTime?>("SubmissionDate"),
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
                                   DateofBirth = model.Field<DateTime>("DateofBirth"),
                                   ProcessedOn = model.Field<DateTime?>("ProcessedOn"),
                                   InvoiceNumber = model.Field<string>("InvoiceNumber"),
                                   InvoiceUrl = model.Field<string>("InvoiceUrl"),
                                   CreatedOn = model.Field<DateTime>("CreatedOn"),



                               }).FirstOrDefault();

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count != 0)
                    {
                        msgBody.subscriberDetails = (from model in ds.Tables[1].AsEnumerable()
                                                     select new SubscriberDetails()
                                                     {
                                                         SubscriberID = model.Field<int?>("SubscriberID"),
                                                         MobileNumber = model.Field<string>("MobileNumber"),
                                                         DisplayName = model.Field<string>("DisplayName"),
                                                         IsPrimary = model.Field<int>("IsPrimary"),
                                                         PremiumType = model.Field<int?>("PremiumType"),
                                                         IsPorted = model.Field<int?>("IsPorted"),

                                                         IsOwnNumber = model.Field<int?>("IsOwnNumber"),
                                                         DonorProvider = model.Field<string>("DonorProvider"),
                                                         DepositFee = model.Field<double?>("DepositFee"),
                                                         IsBuddyLine = model.Field<int?>("IsBuddyLine"),
                                                         LinkedSubscriberID = model.Field<int?>("LinkedSubscriberID"),
                                                         RefOrderSubscriberID = model.Field<int?>("RefOrderSubscriberID"),

                                                         DonorProviderName = model.Field<string>("DonorProviderName"),
                                                         PortedNumberTransferForm = model.Field<string>("PortedNumberTransferForm"),
                                                         PortedNumberOwnedBy = model.Field<string>("PortedNumberOwnedBy"),
                                                         PortedNumberOwnerRegistrationID = model.Field<string>("PortedNumberOwnerRegistrationID"),
                                                     }).FirstOrDefault();

                        if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count != 0)
                            msgBody.subscriberDetails.bundleDetails = (from model in ds.Tables[2].AsEnumerable()
                                                                       select new BundleDetails()
                                                                       {
                                                                           ChangeRequestID = model.Field<int>("ChangeRequestID"),
                                                                           SubscriberID = model.Field<int?>("SubscriberID"),
                                                                           PortalServiceName = model.Field<string>("PortalServiceName"),
                                                                           ServiceFee = model.Field<double?>("ServiceFee"),
                                                                           IsRecurring = model.Field<int?>("IsRecurring"),
                                                                           IsGSTIncluded = model.Field<int?>("IsGSTIncluded"),


                                                                       }).ToList();

                    }
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
            DataTable dt = new DataTable();
            SqlParameter[] parameters =
            {
                    new SqlParameter( "@MPGSOrderID",  SqlDbType.NVarChar )
            };

            parameters[0].Value = MPGSOrderID;

            _DataHelper = new DataAccessHelper(DbObjectNames.CR_GetMessageBody, parameters, _configuration);


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

        public async Task<string> PublishMessageToMessageQueue(string topicName, object msgBody, Dictionary<string,string> messageAttribute, string subject)
        {
            try
            {
                var publisher = new InfrastructureService.MessageQueue.Publisher(_configuration, topicName);
                return await publisher.PublishAsync(msgBody, messageAttribute, subject);
                
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<int> InsertMessageInMessageQueueRequest(MessageQueueRequest messageQueueRequest)
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


    }
}
