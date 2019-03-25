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

        public async Task<List<Bundle>> GetBundleList()
        {
            try
            {               

                _DataHelper = new DataAccessHelper("Catelog_GetBundlesListing", _configuration);

                DataTable dt = new DataTable();

                _DataHelper.Run(dt);

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
                                      PromotionText = model.Field<string>("PromotionText")

                                  }).ToList();
                }

                return bundleList;
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

                _DataHelper.Run(dt);

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

                _DataHelper.Run(dt);

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

        public async Task<DatabaseResponse> CreateBundle(CreateBundleRequest bundle)
        {
            try
            {

                SqlParameter[] parameters =
               {                  
                    new SqlParameter( "@BundleName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@PlanMarketingName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@PortalSummaryDescription",  SqlDbType.NVarChar),
                    new SqlParameter( "@PortalDescription",  SqlDbType.NVarChar ),
                    new SqlParameter( "@IsCustomerSelectable",  SqlDbType.Int ),
                    new SqlParameter( "@ValidFrom",  SqlDbType.Date ),
                    new SqlParameter( "@ValidTo",  SqlDbType.Date )                   
                };

               
                parameters[0].Value = bundle.BundleName;
                parameters[1].Value = bundle.PlanMarketingName;
                parameters[2].Value = bundle.PortalSummaryDescription;
                parameters[3].Value = bundle.PortalDescription;
                parameters[4].Value = bundle.IsCustomerSelectable;
                parameters[5].Value = bundle.ValidFrom;
                parameters[6].Value = bundle.ValidTo;
               

                _DataHelper = new DataAccessHelper("Catelog_CreateBundle", parameters, _configuration);

                DataTable dt = new DataTable();

                int result= _DataHelper.Run(dt);

                Bundle newBundle = new Bundle();

                if (dt.Rows.Count > 0)
                {

                     newBundle = (from model in dt.AsEnumerable()
                                  select new Bundle()
                                  {
                                      BundleID = model.Field<int>("BundleID"),
                                      BundleName = model.Field<string>("BundleName"),
                                      PortalDescription = model.Field<string>("PortalDescription"),
                                      PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                      PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                      //ServiceName = model.Field<string>("ServiceName"),
                                     // ActualServiceFee = model.Field<double>("ActualServiceFee"),
                                     //// ActualSubscriptionFee = model.Field<double>("ActualSubscriptionFee"),
                                     // ApplicableServiceFee = model.Field<double>("ApplicableServiceFee"),
                                      //ApplicableSubscriptionFee = model.Field<double>("ApplicableSubscriptionFee"),
                                      //TotalData = model.Field<double>("TotalData"),
                                     // TotalSMS = model.Field<double>("TotalSMS"),
                                     // TotalVoice = model.Field<double>("TotalVoice")

                                  }).FirstOrDefault();
                }

                return new DatabaseResponse {  ResponseCode=result, Results=newBundle};
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

        public async Task<DatabaseResponse> UpdateBundle(UpdateBundleRequest bundle)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    new SqlParameter( "@BundleName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@PlanMarketingName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@PortalSummaryDescription",  SqlDbType.NVarChar),
                    new SqlParameter( "@PortalDescription",  SqlDbType.NVarChar ),
                    new SqlParameter( "@IsCustomerSelectable",  SqlDbType.Int ),
                    new SqlParameter( "@ValidFrom",  SqlDbType.Date ),
                    new SqlParameter( "@ValidTo",  SqlDbType.Date ),
                    new SqlParameter( "@status",  SqlDbType.Int ),

                };

                parameters[0].Value = bundle.BundleID;
                parameters[1].Value = bundle.BundleName;
                parameters[2].Value = bundle.PlanMarketingName;
                parameters[3].Value = bundle.PortalSummaryDescription;
                parameters[4].Value = bundle.PortalDescription;
                parameters[5].Value = bundle.IsCustomerSelectable;
                parameters[6].Value = bundle.ValidFrom;
                parameters[7].Value = bundle.ValidTo;
                parameters[8].Value = bundle.Status;
               

                _DataHelper = new DataAccessHelper("Catelog_UpdateBundle", parameters, _configuration);

                DataTable dt = new DataTable();

                int result =  _DataHelper.Run(dt);

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
                                      TotalVoice = model.Field<double>("TotalVoice")

                                  }).ToList();
                }

                return new DatabaseResponse { ResponseCode = result, Results = statusList };
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

        public async Task<int> DeleteBundle(int bundleId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@BundleID",  SqlDbType.Int ),
                    //new SqlParameter("@ERROR", SqlDbType.VarChar)
                };

                parameters[0].Value = bundleId;              

                _DataHelper = new DataAccessHelper("Catelog_DeleteBundle", parameters, _configuration);

                DataTable dt = new DataTable();

                return  _DataHelper.Run();
               
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

        public async Task<int> BundleExists(int bundleId)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@BundleID",  SqlDbType.Int )
                    
                };

                parameters[0].Value = bundleId;              

                _DataHelper = new DataAccessHelper("Catelog_BundleExists", parameters, _configuration);

                DataTable dt = new DataTable();

                return _DataHelper.Run();

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
