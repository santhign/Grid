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

namespace OrderService.DataAccess
{
    public class ChangeRequestDataAccess
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
        /// Removes the vas service.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> RemoveVasService(int customerId, string mobileNumber, int planId)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerId",  SqlDbType.Int ),
                    new SqlParameter( "@OldMobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@PlanId",  SqlDbType.Int)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = planId;


                _DataHelper = new DataAccessHelper("Orders_CR_InsertRemoveVAS", parameters, _configuration);



                var result = await _DataHelper.RunAsync();    // 101 / 102 

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
                    new SqlParameter( "@CustomerId",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar ),
                    new SqlParameter( "@BundleID",  SqlDbType.Int),
                    new SqlParameter( "@Quantity",  SqlDbType.Int)
                };

                parameters[0].Value = customerId;
                parameters[1].Value = mobileNumber;
                parameters[2].Value = bundleId;
                parameters[3].Value = quantity;


                _DataHelper = new DataAccessHelper("Orders_CR_BuyVAS", parameters, _configuration);



                var result = await _DataHelper.RunAsync();    // 101 / 102 

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
                parameters[2].Value = Core.Enums.RequestType.ChangeSim.GetDescription();

                _DataHelper = new DataAccessHelper("Order_CR_SIMReplacementRequest", parameters, _configuration);

                var result = await _DataHelper.RunAsync(ds);

                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var TorSresponse = new ChangeSimResponse();

                if (ds.Tables.Count > 0)
                {
                    TorSresponse = (from model in ds.Tables[0].AsEnumerable()
                                    select new ChangeSimResponse()
                                    {
                                        ChangeRequestId = model.Field<int>("ChangeRequestId"),
                                        OrderNumber = model.Field<string>("OrderNumber"),
                                        //RequestOn = model.Field<DateTime>("RequestOn"),
                                        RequestTypeDescription = model.Field<string>("RequestTypeDescription"),
                                        BillingUnit = model.Field<string>("BillingUnit"),
                                        BillingFloor = model.Field<string>("BillingFloor"),
                                        BillingBuildingNumber = model.Field<string>("BillingBuildingNumber"),
                                        BillingStreetName = model.Field<string>("BillingStreetName"),
                                        BillingPostCode = model.Field<string>("BillingPostCode"),
                                        BillingContactNumber = model.Field<string>("BillingContactNumber"),
                                        Name = model.Field<string>("Name"),
                                        Email = model.Field<string>("Email"),
                                        IdentityCardNumber = model.Field<string>("IdentityCardNumber"),
                                        IdentityCardType = model.Field<string>("IdentityCardType"),
                                        //IsSameAsBilling = model.Field<string>("IsSameAsBilling"),
                                        ShippingUnit = model.Field<string>("ShippingUnit"),
                                        ShippingFloor = model.Field<string>("ShippingFloor"),
                                        ShippingBuildingNumber = model.Field<string>("ShippingBuildingNumber"),
                                        ShippingStreetName = model.Field<string>("ShippingStreetName"),
                                        ShippingPostCode = model.Field<string>("ShippingPostCode"),
                                        ShippingContactNumber = model.Field<string>("ShippingContactNumber")//,

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

                _DataHelper = new DataAccessHelper("Orders_CR_RaiseRequest", parameters, _configuration);

                var result = await _DataHelper.RunAsync(ds);

                if (result != (int)Core.Enums.DbReturnValue.CreateSuccess)
                    return new DatabaseResponse { ResponseCode = result };

                var TorSresponse = new TerminationOrSuspensionResponse();

                if (ds.Tables.Count > 0)
                {
                    TorSresponse = (from model in ds.Tables[0].AsEnumerable()
                                    select new TerminationOrSuspensionResponse()
                                    {
                                        ChangeRequestId = model.Field<int>("ChangeRequestId"),
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

                _DataHelper = new DataAccessHelper("Customer_CR_ChangePhoneRequest", parameters, _configuration);

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

        public async Task<DatabaseResponse> AuthenticateCustomerToken(string token)
        {
            try
            {

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@Token",  SqlDbType.NVarChar )

                };

                parameters[0].Value = token;

                _DataHelper = new DataAccessHelper("Customer_AuthenticateToken", parameters, _configuration);

                var dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 111 /109

                DatabaseResponse response = new DatabaseResponse();

                if (result == 111)
                {

                    AuthTokenResponse tokenResponse = new AuthTokenResponse();

                    if (dt != null && dt.Rows.Count > 0)
                    {

                        tokenResponse = (from model in dt.AsEnumerable()
                            select new AuthTokenResponse()
                            {
                                CustomerID = model.Field<int>("CustomerID"),

                                CreatedOn = model.Field<DateTime>("CreatedOn")


                            }).FirstOrDefault();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = tokenResponse };

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
