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
    public class ConfigDataAccess
    {
        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public ConfigDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<DatabaseResponse> GetConfiguration(string configType)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ConfigType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = configType;

                _DataHelper = new DataAccessHelper("Admin_GetConfigurations", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    List<Dictionary<string, string>> configDictionary = new List<Dictionary<string, string>>();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        configDictionary = LinqExtensions.GetDictionary(dt);
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = configDictionary };

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

        public async Task<DatabaseResponse> GetEmailNotificationTemplate(string templateName)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@TemplateName",  SqlDbType.NVarChar )

                };

                parameters[0].Value = templateName;

                _DataHelper = new DataAccessHelper("z_GetEmailTemplateByName", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 109 /105

                DatabaseResponse response = new DatabaseResponse();

                EmailTemplate template = new EmailTemplate();

                if (dt!=null && dt.Rows.Count > 0)
                {
                    template = (from model in dt.AsEnumerable()
                                    select new EmailTemplate()
                                    {
                                         EmailTemplateID = model.Field<int>("EmailTemplateID"),
                                         EmailBody = model.Field<string>("EmailBody"),
                                         EmailSubject = model.Field<string>("EmailSubject"),
                                         TemplateName = model.Field<string>("TemplateName")
                                    }).FirstOrDefault();

                    response = new DatabaseResponse { ResponseCode = result, Results = template };
                }            


                else
                {
                    response = new DatabaseResponse { ResponseCode = result  };
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

        public async Task<DatabaseResponse> CreateEMailNotificationLog(NotificationLog log)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),

                    new SqlParameter( "@Email",  SqlDbType.NVarChar ),

                    new SqlParameter( "@EmailSubject",  SqlDbType.NVarChar ),

                    new SqlParameter( "@EmailBody",  SqlDbType.NVarChar ),

                    new SqlParameter( "@ScheduledOn",  SqlDbType.DateTime ),

                    new SqlParameter( "@EmailTemplateID",  SqlDbType.Int ),

                    new SqlParameter( "@SendOn",  SqlDbType.DateTime ),

                    new SqlParameter( "@Status",  SqlDbType.Int )

                };

                parameters[0].Value = log.CustomerID;
                parameters[1].Value = log.Email;
                parameters[2].Value = log.EmailSubject;
                parameters[3].Value = log.EmailBody;
                parameters[4].Value = log.ScheduledOn;
                parameters[5].Value = log.EmailTemplateID;
                parameters[6].Value = log.SendOn;
                parameters[7].Value = log.Status;
              

                _DataHelper = new DataAccessHelper("z_EmailNotificationsLogEntry", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 107 /100

                DatabaseResponse response = new DatabaseResponse();
               
                response = new DatabaseResponse { ResponseCode = result };
               

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

        public async Task<DatabaseResponse> CreateEMailNotificationLogForDevPurpose(NotificationLogForDevPurpose log)
        {
            try
            {

                SqlParameter[] parameters =
               {

                    new SqlParameter( "@EventType",  SqlDbType.NVarChar ),

                    new SqlParameter( "@Message",  SqlDbType.NVarChar )                
                };

                parameters[0].Value = log.EventType;
                parameters[1].Value = log.Message;
               


                _DataHelper = new DataAccessHelper("z_EmailNotificationsLogEntryForDevPurpose", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 107 /100

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };


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

        public async Task<DatabaseResponse> GetSMSNotificationTemplate(string templateName)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@TemplateName",  SqlDbType.NVarChar )

                };

                parameters[0].Value = templateName;

                _DataHelper = new DataAccessHelper("z_GetSMSTemplateByName", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 109 /105

                DatabaseResponse response = new DatabaseResponse();

                SMSTemplates template = new SMSTemplates();

                if (dt != null && dt.Rows.Count > 0)
                {
                    template = (from model in dt.AsEnumerable()
                                select new SMSTemplates()
                                {
                                    SMSTemplateID = model.Field<int>("SMSTemplateID"),
                                    TemplateName = model.Field<string>("TemplateName"),
                                    SMSTemplate = model.Field<string>("SMSTemplate")
                                }).FirstOrDefault();

                    response = new DatabaseResponse { ResponseCode = result, Results = template };
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

        public async Task<DatabaseResponse> CreateSMSNotificationLog(SMSNotificationLog log)
        {
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Email",  SqlDbType.NVarChar ),

                    new SqlParameter( "@SMSText",  SqlDbType.NVarChar ),

                    new SqlParameter( "@Mobile",  SqlDbType.NVarChar ),

                    new SqlParameter( "@ScheduledOn",  SqlDbType.DateTime ),

                    new SqlParameter( "@SMSTemplateID",  SqlDbType.Int ),

                    new SqlParameter( "@SendOn",  SqlDbType.DateTime ),

                    new SqlParameter( "@Status",  SqlDbType.Int )

                };

                parameters[0].Value = log.Email;
                parameters[1].Value = log.SMSText;
                parameters[2].Value = log.Mobile;               
                parameters[4].Value = log.ScheduledOn;
                parameters[5].Value = log.SMSTemplateID;
                parameters[6].Value = log.SendOn;
                parameters[7].Value = log.Status;


                _DataHelper = new DataAccessHelper("z_EmailNotificationsLogEntry", parameters, _configuration);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 107 /100

                DatabaseResponse response = new DatabaseResponse();

                response = new DatabaseResponse { ResponseCode = result };


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
