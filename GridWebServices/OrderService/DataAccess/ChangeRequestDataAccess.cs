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

namespace OrderService.DataAccess
{
    public class ChangeRequestDataAccess : IChangeRequestDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public ChangeRequestDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Remove VAS Data Access method
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="mobileNumber"></param>
        /// <param name="subscriptionID"></param>
        /// <param name="planId"></param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RemoveVasService(int customerId, string mobileNumber, int subscriptionID, int planId)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@SubscriptionID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@PlanID",  SqlDbType.Int),
                    new SqlParameter( "@RequestType",  SqlDbType.NVarChar )
                };

                parameters[0].Value = customerId;
                parameters[1].Value = subscriptionID;
                parameters[2].Value = mobileNumber;
                parameters[3].Value = planId;
                parameters[4].Value = Core.Enums.RequestType.RemoveVAS.GetDescription();


                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_CR_InsertRemoveVAS, parameters, _configuration);

                DataTable dt = new DataTable();
                var result = await _DataHelper.RunAsync(dt);    // 101 / 102 

                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var removeVasResponse = new RemoveVASResponse();

                if (dt.Rows.Count > 0)
                {
                    removeVasResponse = (from model in dt.AsEnumerable()
                                         select new RemoveVASResponse()
                                         {
                                             ChangeRequestID = model.Field<int>("ChangeRequestID"),
                                             BSSPlanCode = model.Field<string>("BSSPlanCode"),
                                             PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                             CurrentDate = model.Field<DateTime>("CurrentDate"),
                                             PlanID = model.Field<int>("PlanID")
                                         }).FirstOrDefault();
                }
                else
                {
                    removeVasResponse = null;
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = removeVasResponse };
                return response;
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
        /// Buys the vas service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="bundleId">The bundle identifier.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> BuyVasService(int customerId, string mobileNumber, int bundleId, int quantity)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int),
                    new SqlParameter( "@Quantity",  SqlDbType.Int),
                    new SqlParameter( "@RequestType",  SqlDbType.NVarChar)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = bundleId;
                parameters[3].Value = quantity;
                parameters[4].Value = Core.Enums.RequestType.AddVAS.GetDescription();


                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_CR_BuyVAS, parameters, _configuration);
                DataTable dt = new DataTable();
                var result = await _DataHelper.RunAsync(dt);    // 101 / 102
                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var removeVASResponse = new BuyVASResponse();

                if (dt.Rows.Count > 0)
                {
                    removeVASResponse = (from model in dt.AsEnumerable()
                                         select new BuyVASResponse()
                                         {
                                             ChangeRequestID = model.Field<int>("ChangeRequestID"),
                                             BSSPlanCode = model.Field<string>("BSSPlanCode"),
                                             PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                             SubscriptionFee = model.Field<double>("SubscriptionFee")
                                         }).FirstOrDefault();
                }
                else
                {
                    removeVASResponse = null;
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = removeVASResponse };
                return response;
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
        /// Sims the replacement request.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> SimReplacementRequest(int customerId, string mobileNumber)
        {
            try
            {
                var ds = new DataSet();
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@RequestType",  SqlDbType.NVarChar)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = Core.Enums.RequestType.ReplaceSIM.GetDescription();

                _DataHelper = new DataAccessHelper(DbObjectNames.Order_CR_SIMReplacementRequest, parameters, _configuration);

                var result = await _DataHelper.RunAsync(ds);

                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var TorSresponse = new ChangeSimResponse();

                if (ds.Tables.Count > 0)
                {
                    TorSresponse = (from model in ds.Tables[0].AsEnumerable()
                                    select new ChangeSimResponse()
                                    {
                                        ChangeRequestId = model.Field<int>("ChangeRequestID"),
                                        OrderNumber = model.Field<string>("OrderNumber"),
                                        RequestOn = model.Field<DateTime>("RequestOn"),
                                        RequestTypeDescription = model.Field<string>("RequestTypeDescription"),
                                        BillingUnit = model.Field<string>("BillingUnit"),
                                        BillingFloor = model.Field<string>("BillingFloor"),
                                        BillingBuildingName = model.Field<string>("BillingBuildingName"),
                                        BillingBuildingNumber = model.Field<string>("BillingBuildingNumber"),
                                        BillingStreetName = model.Field<string>("BillingStreetName"),
                                        BillingPostCode = model.Field<string>("BillingPostCode"),
                                        BillingContactNumber = model.Field<string>("BillingContactNumber"),
                                        Name = model.Field<string>("Name"),
                                        Email = model.Field<string>("Email"),
                                        IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                        IdentityCardType = model.Field<string>("IdentityCardType"),
                                        IsSameAsBilling = model.Field<int>("IsSameAsBilling"),
                                        ShippingUnit = model.Field<string>("ShippingUnit"),
                                        ShippingFloor = model.Field<string>("ShippingFloor"),
                                        ShippingBuildingNumber = model.Field<string>("ShippingBuildingNumber"),
                                        ShippingStreetName = model.Field<string>("ShippingStreetName"),
                                        ShippingPostCode = model.Field<string>("ShippingPostCode"),
                                        ShippingContactNumber = model.Field<string>("ShippingContactNumber"),
                                        AlternateRecipientName = model.Field<string>("AlternateRecipientName"),
                                        AlternateRecipientEmail = model.Field<string>("AlternateRecipientEmail"),
                                        AlternateRecipientContact = model.Field<string>("AlternateRecipientContact"),
                                        AlternateRecipientIDNumber = model.Field<string>("AlternateRecipientIDNumber"),
                                        AlternateRecipientIDType = model.Field<string>("AlternateRecioientIDType"),
                                        PortalSlotID = model.Field<string>("PortalSlotID"),
                                        ScheduledDate = model.Field<DateTime?>("ScheduledDate"),
                                        DeliveryVendor = model.Field<string>("DeliveryVendor"),
                                        DeliveryOn = model.Field<DateTime?>("DeliveryOn"),
                                        DeliveryTime = model.Field<DateTime?>("DeliveryTime"),
                                        VendorTrackingCode = model.Field<string>("VendorTrackingCode"),
                                        VendorTrackingUrl = model.Field<string>("VendorTrackingUrl"),
                                        DeliveryFee = model.Field<double>("DeliveryFee")

                                    }).FirstOrDefault();

                    if (TorSresponse != null)
                        TorSresponse.ChangeRequestChargesList = (from model in ds.Tables[1].AsEnumerable()
                                                                 select new ChangeRequestCharges()
                                                                 {
                                                                     PortalServiceName = model.Field<string>("PortalServiceName"),
                                                                     ServiceFee = model.Field<double>("ServiceFee"),
                                                                     IsRecurring = model.Field<bool>("IsRecurring"),
                                                                     IsGstIncluded = model.Field<bool>("IsGSTIncluded")
                                                                 }).ToList();
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = TorSresponse };
                return response;
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
        /// Terminations the or suspension request.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="request">The request.</param>
        /// <param name="remark">The remark.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> TerminationOrSuspensionRequest(int customerId, string mobileNumber, string request, string remark)
        {
            try
            {
                var ds = new DataSet();
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@RequestTypeDescription",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Remarks",  SqlDbType.NVarChar)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = request;
                parameters[3].Value = remark;

                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_CR_RaiseRequest, parameters, _configuration);

                var result = await _DataHelper.RunAsync(ds);

                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var TorSresponse = new TerminationOrSuspensionResponse();

                if (ds.Tables.Count > 0)
                {
                    TorSresponse = (from model in ds.Tables[0].AsEnumerable()
                                    select new TerminationOrSuspensionResponse()
                                    {
                                        ChangeRequestId = model.Field<int>("ChangeRequestID"),
                                        OrderNumber = model.Field<string>("OrderNumber"),
                                        RequestOn = model.Field<DateTime>("RequestOn"),
                                        RequestTypeDescription = model.Field<string>("RequestTypeDescription")
                                    }).FirstOrDefault();

                    if (TorSresponse != null)
                        TorSresponse.ChangeRequestChargesList = (from model in ds.Tables[1].AsEnumerable()
                                                                 select new ChangeRequestCharges()
                                                                 {
                                                                     PortalServiceName = model.Field<string>("PortalServiceName"),
                                                                     ServiceFee = model.Field<double>("ServiceFee")
                                                                 }).ToList();
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = TorSresponse };
                return response;
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
        /// Changes the phone request.
        /// </summary>
        /// <param name="changePhone">The change phone.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> ChangePhoneRequest(ChangePhoneRequest changePhone)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.NVarChar ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@NewMobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@RequestTypeDescription",  SqlDbType.NVarChar),
                    new SqlParameter( "@PremiumType",  SqlDbType.Int),
                    //new SqlParameter( "@PortedNumberTransferForm",  SqlDbType.NVarChar),
                    //new SqlParameter( "@PortedNumberOwnedBy",  SqlDbType.NVarChar),
                    //new SqlParameter( "@PortedNumberOwnerRegistrationID",  SqlDbType.NVarChar)
                    
                };

                parameters[0].Value = changePhone.CustomerId;
                parameters[1].Value = changePhone.MobileNumber;
                parameters[2].Value = changePhone.NewMobileNumber;
                parameters[3].Value = Core.Enums.RequestType.ChangeNumber.GetDescription();
                parameters[4].Value = changePhone.PremiumType;
                //parameters[5].Value = changePhone.PortedNumberTransferForm;
                //parameters[6].Value = changePhone.PortedNumberOwnedBy;
                //parameters[7].Value = changePhone.PortedNumberOwnerRegistrationId;

                _DataHelper = new DataAccessHelper(DbObjectNames.Customer_CR_ChangePhoneRequest, parameters, _configuration);

                var result = await _DataHelper.RunAsync();

                return new DatabaseResponse { ResponseCode = result };
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

        public async Task<DatabaseResponse> UpdateCRShippingDetails(UpdateCRShippingDetailsRequest shippingDetails)
        {
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ChangeRequestID",  SqlDbType.Int ),
                    new SqlParameter( "@Postcode",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BlockNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@Unit",  SqlDbType.NVarChar),
                    new SqlParameter( "@Floor",  SqlDbType.NVarChar),
                    new SqlParameter( "@BuildingName",  SqlDbType.NVarChar),
                    new SqlParameter( "@StreetName",  SqlDbType.NVarChar),
                    new SqlParameter( "@ContactNumber",  SqlDbType.NVarChar),
                    new SqlParameter( "@IsBillingSame",  SqlDbType.Int),
                    new SqlParameter( "@PortalSlotID",  SqlDbType.NVarChar),
                    new SqlParameter( "@CustomerID", SqlDbType.Int)
                };

                parameters[0].Value = shippingDetails.ChangeRequestID;
                parameters[1].Value = shippingDetails.Postcode;
                parameters[2].Value = shippingDetails.BlockNumber;
                parameters[3].Value = shippingDetails.Unit;
                parameters[4].Value = shippingDetails.Floor;
                parameters[5].Value = shippingDetails.BuildingName;
                parameters[6].Value = shippingDetails.StreetName;
                parameters[7].Value = shippingDetails.ContactNumber;
                parameters[8].Value = shippingDetails.IsBillingSame;
                parameters[9].Value = shippingDetails.PortalSlotID;
                parameters[10].Value = shippingDetails.CustomerID;


                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_CR_UpdateCRShippingDetails, parameters, _configuration);

                int result = await _DataHelper.RunAsync();    // 101 / 109 

                return new DatabaseResponse { ResponseCode = result };
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
        /// Buys the shared service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="bundleId">The bundle identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> BuySharedService(int customerId, int bundleId)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int),
                    new SqlParameter( "@RequestType",  SqlDbType.NVarChar)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = bundleId;
                parameters[2].Value = Core.Enums.RequestType.AddVAS.GetDescription();


                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_CR_BuySharedVAS, parameters, _configuration);
                DataTable dt = new DataTable();
                var result = await _DataHelper.RunAsync(dt);    // 101 / 102
                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var removeVASResponse = new BuyVASResponse();

                if (dt.Rows.Count > 0)
                {
                    removeVASResponse = (from model in dt.AsEnumerable()
                                         select new BuyVASResponse()
                                         {
                                             ChangeRequestID = model.Field<int>("ChangeRequestID"),
                                             BSSPlanCode = model.Field<string>("BSSPlanCode"),
                                             PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                             SubscriptionFee = model.Field<double>("SubscriptionFee")
                                         }).FirstOrDefault();
                }
                else
                {
                    removeVASResponse = null;
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = removeVASResponse };
                return response;
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
        /// Remove Shared VAS Service 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="accountSubscriptionId"></param>
        /// <param name="planId"></param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RemoveSharedVasService(int customerId, int accountSubscriptionId, int planId)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ), 
                    new SqlParameter( "@AccountSubscriptionID",  SqlDbType.Int ),
                    new SqlParameter( "@PlanID",  SqlDbType.Int),
                    new SqlParameter( "@RequestType",  SqlDbType.NVarChar )
                };

                parameters[0].Value = customerId;
                parameters[1].Value = accountSubscriptionId;
                parameters[2].Value = planId;
                parameters[3].Value = Core.Enums.RequestType.RemoveVAS.GetDescription();


                _DataHelper = new DataAccessHelper(DbObjectNames.Orders_CR_InsertRemoveSharedVAS, parameters, _configuration);

                DataTable dt = new DataTable();
                var result = await _DataHelper.RunAsync(dt);    // 101 / 102 

                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var removeVasResponse = new RemoveVASResponse();

                if (dt.Rows.Count > 0)
                {
                    removeVasResponse = (from model in dt.AsEnumerable()
                                         select new RemoveVASResponse()
                                         {
                                             ChangeRequestID = model.Field<int>("ChangeRequestID"),
                                             BSSPlanCode = model.Field<string>("BSSPlanCode"),
                                             PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                             CurrentDate = model.Field<DateTime>("CurrentDate"),
                                             PlanID = model.Field<int>("PlanID")
                                         }).FirstOrDefault();
                }
                else
                {
                    removeVasResponse = null;
                }

                var response = new DatabaseResponse { ResponseCode = result, Results = removeVasResponse };
                return response;
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
    }
}
