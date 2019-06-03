using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Core.Helpers;
using CatelogService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using InfrastructureService;
using Core.Enums;

namespace CatelogService.DataAccess
{
    public class VASDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public VASDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<VAS>> GetVASes(int CustomerID)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.VarChar )
                };

                parameters[0].Value = CustomerID;
                _DataHelper = new DataAccessHelper("Catelog_GetVASListing", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<VAS> vasList = new List<VAS>();

                if (dt.Rows.Count > 0)
                {

                    vasList = (from model in dt.AsEnumerable() 
                                  select new VAS()
                                  {
                                      VASID = model.Field<int>("VASID"),
                                       PortalDescription = model.Field<string>("PortalDescription"),
                                       PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                       PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                       Data = model.Field<double>("Data"),
                                       SMS = model.Field<double>("SMS"),
                                       Voice = model.Field<double>("Voice"),
                                       SubscriptionFee = model.Field<double>("SubscriptionFee"),
                                       IsRecurring = model.Field<string>("IsRecurring"),
                                       SubscriptionCount = model.Field<int>("SubscriptionCount")
                                  }).ToList();
                }

                return vasList;
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

        public async Task<List<VAS>> GetVASForPurchase(int CustomerID, string MobileNumber)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.VarChar ),
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = MobileNumber;
                _DataHelper = new DataAccessHelper("Catelog_GetPurchaseVASListing", parameters, _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<VAS> vasList = new List<VAS>();

                if (dt.Rows.Count > 0)
                {

                    vasList = (from model in dt.AsEnumerable()
                               select new VAS()
                               {
                                   VASID = model.Field<int>("VASID"),
                                   PortalDescription = model.Field<string>("PortalDescription"),
                                   PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                   PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                   Data = model.Field<double>("Data"),
                                   SMS = model.Field<double>("SMS"),
                                   Voice = model.Field<double>("Voice"),
                                   SubscriptionFee = model.Field<double>("SubscriptionFee"),
                                   IsRecurring = model.Field<string>("IsRecurring"),
                                   SubscriptionCount = model.Field<int>("SubscriptionCount")
                               }).ToList();
                }

                return vasList;
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


        public async Task<List<VAS>> GetDefaultVASes()
        {
            try
            {
                //SqlParameter[] parameters =
                //{
                //    new SqlParameter( "@CustomerID",  SqlDbType.VarChar )
                //};

                //parameters[0].Value = CustomerID;
                _DataHelper = new DataAccessHelper("Catelog_GetDefaultPlans", _configuration);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                List<VAS> vasList = new List<VAS>();

                if (dt.Rows.Count > 0)
                {

                    vasList = (from model in dt.AsEnumerable()
                               select new VAS()
                               {
                                   VASID = model.Field<int>("PlanID"),
                                   PortalDescription = model.Field<string>("PortalDescription"),
                                   PlanMarketingName = model.Field<string>("PlanMarketingName"),
                                   PortalSummaryDescription = model.Field<string>("PortalSummaryDescription"),
                                   Data = model.Field<double>("Data"),
                                   SMS = model.Field<double>("SMS"),
                                   Voice = model.Field<double>("Voice"),
                                   SubscriptionFee = model.Field<double>("SubscriptionFee"),
                                   IsRecurring = model.Field<string>("IsRecurring"),
                                   //SubscriptionCount = model.Field<int>("SubscriptionCount")
                               }).ToList();
                }

                return vasList;
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
