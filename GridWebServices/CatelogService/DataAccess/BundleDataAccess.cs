using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Core.Helpers;
using CatelogService.Models;
using Core.Models;
using Core.Enums;
using InfrastructureService;



namespace CatelogService.DataAccess
{
    public class BundleDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public BundleDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Bundle>> GetBundleList(string code)
        {
            try
            {
                if (code != "")
                {
                    SqlParameter[] parameters =
                    {
                    new SqlParameter( "@UserCode",  SqlDbType.NVarChar )
                    };
                    parameters[0].Value = code;

                    _DataHelper = new DataAccessHelper("Catelog_GetBundlesListing", parameters, _configuration);
                }
                else
                {
                    _DataHelper = new DataAccessHelper("Catelog_GetBundlesListing", _configuration);
                }
                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<Bundle> bundleList = new List<Bundle>();

                if (dt.Rows.Count > 0)
                {

                    bundleList = (from model in dt.AsEnumerable()
                                  select new Bundle()
                                  {
                                      BundleID = model.Field<int>("BundleID"),
                                      BundleName = model.Field<string>("BundleName"),
                                      PortalDescription = model.Field<string>("PortalDescription"),
                                      PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                      PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                      ServiceName = model.Field<string>("ServiceName"),
                                      ActualServiceFee = model.Field<double>("ActualServiceFee"),
                                      ActualSubscriptionFee = model.Field<double>("ActualSubscriptionFee"),
                                      ApplicableServiceFee = model.Field<double>("ApplicableServiceFee"),
                                      ApplicableSubscriptionFee = model.Field<double>("ApplicableSubscriptionFee"),
                                      TotalData = model.Field<double>("TotalData"),
                                      TotalSMS = model.Field<double>("TotalSMS"),
                                      TotalVoice = model.Field<double>("TotalVoice"),
                                      PromotionText = model.Field<string>("PromotionText"),
                                      PricingDescription = model.Field<string>("PricingDescription")

                                  }).ToList();
                }

                return bundleList;
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

        public async Task<List<Bundle>> GetBundleById(int bundleId)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@BundleID",  SqlDbType.Int )
                   
                };

                parameters[0].Value = bundleId;

                _DataHelper = new DataAccessHelper("Catelog_GetBundleById", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<Bundle> statusList = new List<Bundle>();

                if (dt.Rows.Count > 0)
                {

                    statusList = (from model in dt.AsEnumerable()
                                  select new Bundle()
                                  {
                                      BundleID = model.Field<int>("BundleID"),
                                      BundleName = model.Field<string>("BundleName"),
                                      PortalDescription = model.Field<string>("PortalDescription"),
                                      PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                      PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                      ServiceName = model.Field<string>("ServiceName"),
                                      ActualServiceFee = model.Field<double>("ActualServiceFee"),
                                      ActualSubscriptionFee = model.Field<double>("ActualSubscriptionFee"),
                                      ApplicableServiceFee = model.Field<double>("ApplicableServiceFee"),
                                      ApplicableSubscriptionFee = model.Field<double>("ApplicableSubscriptionFee"),
                                      TotalData = model.Field<double>("TotalData"),
                                      TotalSMS = model.Field<double>("TotalSMS"),
                                      TotalVoice = model.Field<double>("TotalVoice"),
                                      PromotionText = model.Field<string>("PromotionText"),
                                      PricingDescription = model.Field<string>("PricingDescription")

                                  }).ToList();
                }

                return statusList;
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

        public async Task<List<Bundle>> GetBundleByPromocode(int bundleId, string promocode)
        {
            try
            {
              
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    new SqlParameter( "@PromotionCode",  SqlDbType.VarChar )
                };

                parameters[0].Value = bundleId;
                parameters[1].Value = promocode;

                _DataHelper = new DataAccessHelper("Catelog_GetPromotionalBundle", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<Bundle> statusList = new List<Bundle>();

                if (dt.Rows.Count > 0)
                {

                    statusList = (from model in dt.AsEnumerable()
                                  select new Bundle()
                                  {
                                      BundleID = model.Field<int>("BundleID"),
                                      BundleName = model.Field<string>("BundleName"),
                                      PortalDescription = model.Field<string>("PortalDescription"),
                                      PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                      PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                      ServiceName = model.Field<string>("ServiceName"),
                                      ActualServiceFee = model.Field<double>("ActualServiceFee"),
                                      ActualSubscriptionFee = model.Field<double>("ActualSubscriptionFee"),
                                      ApplicableServiceFee = model.Field<double>("ApplicableServiceFee"),
                                      ApplicableSubscriptionFee = model.Field<double>("ApplicableSubscriptionFee"),
                                      TotalData = model.Field<double>("TotalData"),
                                      TotalSMS = model.Field<double>("TotalSMS"),
                                      TotalVoice = model.Field<double>("TotalVoice"),
                                      PromotionText = model.Field<string>("PromotionText")
                                  }).ToList();
                }

                return statusList;
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


        public async Task<DatabaseResponse> GetCustomerBundleListing(int CustomerID, string MobileNumber)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar )
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = MobileNumber;

                _DataHelper = new DataAccessHelper("Customers_GetBundlesListing", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 105 /102

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {
                    var _plan = new List<Bundle>();

                    if (dt.Rows.Count > 0)
                    {

                        _plan = (from model in dt.AsEnumerable()
                                 select new Bundle()
                                 {
                                     BundleID = model.Field<int>("BundleID"),
                                     PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                     PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                     PortalDescription = model.Field<string>("PortalDescription"),
                                     TotalData = model.Field<double>("TotalData"),
                                     TotalSMS = model.Field<double>("TotalSMS"),
                                     TotalVoice = model.Field<double>("TotalVoice"),
                                     ActualSubscriptionFee = model.Field<double>("ActualSubscriptionFee"),
                                     ApplicableSubscriptionFee = model.Field<double>("ApplicableSubscriptionFee"),
                                     PricingDescription = model.Field<string>("PricingDescription"),
                                 }).ToList();
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = _plan };
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
