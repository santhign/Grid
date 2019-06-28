using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Extensions;
using InfrastructureService;
using AdminService.DataAccess;
using AdminService.Models;
using Microsoft.Extensions.Configuration;
using AdminService.DataAccess.Interfaces;
using InfrastructureService.MessageQueue;
using Newtonsoft.Json;
using Core.DataAccess;
using System.IO;

namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminOrderController : Controller
    {
        /// <summary>
        /// The iconfiguration
        /// </summary>
        private readonly IConfiguration _iconfiguration;

        /// <summary>
        /// The admin order data access
        /// </summary>
        private readonly IAdminOrderDataAccess _adminOrderDataAccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminOrderController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="adminOrderDataAccess">The admin order data access.</param>
        public AdminOrderController(IConfiguration configuration, IAdminOrderDataAccess adminOrderDataAccess)
        {
            _iconfiguration = configuration;
            _adminOrderDataAccess = adminOrderDataAccess;
        }

        /// <summary>
        /// Gets the orders list.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="deliveryStatus">The delivery status.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        [HttpGet("GetOrdersListForNRIC")]
        public async Task<IActionResult> GetOrdersList([FromHeader(Name = "Grid-Authorization-Token")] string token, string deliveryStatus, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (!ModelState.IsValid)
                        {
                            return StatusCode((int)HttpStatusCode.OK,
                                new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = true,
                                    Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                                });
                        }
                        int? deliveryStatusNumber = null;
                        if (!string.IsNullOrWhiteSpace(deliveryStatus))
                        {
                            if (deliveryStatus.Trim().ToLower() == IDVerificationStatus.PendingVerification.GetDescription().Trim().ToLower())
                                deliveryStatusNumber = 0;
                            else if (deliveryStatus.Trim().ToLower() == IDVerificationStatus.AcceptedVerification.GetDescription().Trim().ToLower())
                                deliveryStatusNumber = 1;
                            else if (deliveryStatus.Trim().ToLower() == IDVerificationStatus.RejectedVerification.GetDescription().Trim().ToLower())
                                deliveryStatusNumber = 2;

                        }

                        var orderList = await _adminOrderDataAccess.GetOrdersList(deliveryStatusNumber, fromDate, toDate);

                        if (orderList == null || orderList.Count == 0)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)

                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = orderList

                            });
                        }

                    }
                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }

                }

                else
                {
                    // token auth failure
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = false
                    });
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }

        }

        /// <summary>
        /// Gets the order details.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="orderID">The order identifier.</param>
        /// <returns></returns>
        [HttpGet("GetOrderDetailsForNRIC/{orderID}")]
        public async Task<IActionResult> GetOrderDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int orderID)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (!ModelState.IsValid)
                        {
                            return StatusCode((int)HttpStatusCode.OK,
                                new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = true,
                                    Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                                });
                        }

                        CommonDataAccess commonData = new CommonDataAccess(_iconfiguration);

                        var orderList = await commonData.GetOrderDetails(orderID);

                        if (orderList == null || orderList.OrderID == 0)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)

                            });
                        }
                        else
                        {
                            // DownloadFile


                            DatabaseResponse awsConfigResponse = await commonData.GetConfiguration(ConfiType.AWS.ToString());

                            if (awsConfigResponse != null && awsConfigResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                MiscHelper configHelper = new MiscHelper();

                                GridAWSS3Config awsConfig = configHelper.GetGridAwsConfig((List<Dictionary<string, string>>)awsConfigResponse.Results);

                                AmazonS3 s3Helper = new AmazonS3(awsConfig);


                                DownloadResponse FrontImageDownloadResponse = new DownloadResponse();

                                DownloadResponse BackImageDownloadResponse = new DownloadResponse();

                                if (!string.IsNullOrEmpty(orderList.DocumentURL))
                                {
                                    FrontImageDownloadResponse = await s3Helper.DownloadFile(orderList.DocumentURL.Remove(0, awsConfig.AWSEndPoint.Length));

                                    if (FrontImageDownloadResponse.HasSucceed)
                                    {
                                        orderList.FrontImage = FrontImageDownloadResponse.FileObject != null ? configHelper.GetBase64StringFromByteArray(FrontImageDownloadResponse.FileObject, orderList.DocumentURL.Remove(0, awsConfig.AWSEndPoint.Length)) : null;
                                        orderList.DocumentURL = "";
                                    }
                                    else
                                    {
                                        orderList.DocumentURL = "";
                                        orderList.FrontImage = "";
                                    }
                                }

                                if (!string.IsNullOrEmpty(orderList.DocumentBackURL))
                                {
                                    BackImageDownloadResponse = await s3Helper.DownloadFile(orderList.DocumentBackURL.Remove(0, awsConfig.AWSEndPoint.Length));

                                    if (BackImageDownloadResponse.HasSucceed)
                                    {

                                        orderList.BackImage = BackImageDownloadResponse.FileObject != null ? configHelper.GetBase64StringFromByteArray(BackImageDownloadResponse.FileObject, orderList.DocumentBackURL.Remove(0, awsConfig.AWSEndPoint.Length)) : null;
                                        orderList.DocumentBackURL = "";
                                    }
                                    else
                                    {
                                        orderList.DocumentBackURL = "";
                                        orderList.BackImage = "";
                                    }
                                }
                                return Ok(new ServerResponse
                                {
                                    HasSucceeded = true,
                                    Message = StatusMessages.SuccessMessage,
                                    Result = orderList

                                });
                            }
                            else
                            {
                                // unable to get aws config
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration));

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration)

                                });
                            }

                        }

                    }
                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }

                }

                else
                {
                    // token auth failure
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = false
                    });
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }

        }

        /// <summary>
        /// Updates the nric details.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost("UpdateNRICDetails")]
        public async Task<IActionResult> UpdateNRICDetails([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromForm] NRICDetails request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (!ModelState.IsValid)
                        {
                            return StatusCode((int)HttpStatusCode.OK,
                                new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = true,
                                    Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                                });
                        }

                        if (!string.IsNullOrEmpty(request.IdentityCardNumber)) { 
                            EmailValidationHelper _helper = new EmailValidationHelper();
                            if (!_helper.NRICValidation(null, request.IdentityCardNumber, out string _warningmsg))
                            {
                                LogInfo.Warning("NRIC Validation with type: " + _warningmsg);
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = "Document details are invalid",
                                    IsDomainValidationErrors = false
                                });
                            }
                        }

                        int deliveryStatusNumber = request.IDVerificationStatus;
                        

                        var authToken = (AuthTokenResponse)tokenAuthResponse.Results;
                        MiscHelper configHelper = new MiscHelper();
                        CommonDataAccess _commonDataAccess = new CommonDataAccess(_iconfiguration);

                        NRICDetailsRequest personalDetails = new NRICDetailsRequest
                        {
                            OrderID = request.OrderID,
                            IdentityCardNumber = request.IdentityCardNumber,
                            IdentityCardType = request.IdentityCardType,
                            Nationality = request.Nationality,
                            NameInNRIC = request.NameInNRIC,
                            DOB = request.DOB,
                            Expiry = request.Expiry,
                            Remarks = request.Remarks,
                            IDVerificationStatus = request.IDVerificationStatus,

                        };

                        if (request.FrontImage != null || request.BackImage != null)
                        {
                            string IDCardNumberForImage = string.Empty;

                            DatabaseResponse awsConfigResponse = await _commonDataAccess.GetConfiguration(ConfiType.AWS.ToString());

                            if (awsConfigResponse != null && awsConfigResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                            {

                                GridAWSS3Config awsConfig = configHelper.GetGridAwsConfig((List<Dictionary<string, string>>)awsConfigResponse.Results);
                                // Check for IdentityCardNumber 
                                //Start
                                if (string.IsNullOrEmpty(request.IdentityCardNumber))
                                {
                                    var orderDetailsForIDCard = await _commonDataAccess.GetOrderDetails(request.OrderID);
                                    IDCardNumberForImage = orderDetailsForIDCard.IdentityCardNumber;
                                }
                                else
                                {
                                    IDCardNumberForImage = request.IdentityCardNumber;
                                }
                                //End
                                AmazonS3 s3Helper = new AmazonS3(awsConfig);
                                if (request.FrontImage != null)
                                {
                                    string fileNameFront = IDCardNumberForImage.Substring(1, IDCardNumberForImage.Length - 2) +
                                        "_Front_" + DateTime.Now.ToString("yyMMddhhmmss") + Path.GetExtension(request.FrontImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                                    UploadResponse s3UploadResponse = await s3Helper.UploadFile(request.FrontImage, fileNameFront);

                                    if (s3UploadResponse.HasSucceed)
                                    {
                                        personalDetails.FrontImage = awsConfig.AWSEndPoint + s3UploadResponse.FileName;
                                    }
                                    else
                                    {
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                                    }
                                }
                                if (request.BackImage != null)
                                {
                                    string fileNameBack = IDCardNumberForImage.Substring(1, IDCardNumberForImage.Length - 2) + "_Back_" + DateTime.Now.ToString("yyMMddhhmmss")
                                        + Path.GetExtension(request.BackImage.FileName); //Grid_IDNUMBER_yyyymmddhhmmss.extension

                                    UploadResponse s3UploadResponse = await s3Helper.UploadFile(request.BackImage, fileNameBack);

                                    if (s3UploadResponse.HasSucceed)
                                    {
                                        personalDetails.BackImage = awsConfig.AWSEndPoint + s3UploadResponse.FileName;
                                    }
                                    else
                                    {
                                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.S3UploadFailed));
                                    }
                                }
                            }
                            else
                            {
                                // unable to get aws config
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetConfiguration));

                            }
                        }

                        var returnResponse = await _commonDataAccess.UpdateNRICDetails(authToken.CustomerID, deliveryStatusNumber, personalDetails);

                        if (returnResponse.ResponseCode == (int)DbReturnValue.UpdateSuccessSendEmail)
                        {
                            var emailDetails = (EmailResponse)returnResponse.Results;
                            DatabaseResponse configResponse = new DatabaseResponse();
                            DatabaseResponse tokenCreationResponse = new DatabaseResponse();

                            string finalURL = string.Empty;
                            // Fetch the URL
                            if (emailDetails.VerificationStatus == 2) // Rejected then token
                            {

                                configResponse = ConfigHelper.GetValueByKey(ConfigKeys.NRICReUploadLink.GetDescription(), _iconfiguration);
                                tokenCreationResponse = await _adminOrderDataAccess.CreateTokenForVerificationRequests(request.OrderID);
                                var tokenCreation = (VerificationRequestResponse)tokenCreationResponse.Results;
                                finalURL = configResponse.Results.ToString() + tokenCreation.RequestToken;
                            }
                            else
                            {
                                var result = await _commonDataAccess.UpdateTokenForVerificationRequests(request.OrderID);
                            }

                            //Sending message start
                            // Send email to customer email                            ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);
                            DatabaseResponse registrationResponse = await _adminOrderDataAccess.GetEmailNotificationTemplate(emailDetails.VerificationStatus == 2 ? NotificationEvent.ICValidationReject.GetDescription() : NotificationEvent.ICValidationChange.GetDescription());

                            var notificationMessage = MessageHelper.GetMessage(emailDetails.Email, emailDetails.Name, emailDetails.VerificationStatus == 2 ? NotificationEvent.ICValidationReject.GetDescription() : NotificationEvent.ICValidationChange.GetDescription(),
                           ((EmailTemplate)registrationResponse.Results).TemplateName,
                       _iconfiguration, string.IsNullOrWhiteSpace(finalURL) ? "-" : finalURL, string.IsNullOrWhiteSpace(emailDetails.Remark) ? "-" : emailDetails.Remark,  string.IsNullOrWhiteSpace(emailDetails.ChangeLog) ? "-" : emailDetails.ChangeLog);
                            var notificationResponse = await _adminOrderDataAccess.GetConfiguration(ConfiType.Notification.ToString());


                            MiscHelper parser = new MiscHelper();
                            var notificationConfig = parser.GetNotificationConfig((List<Dictionary<string, string>>)notificationResponse.Results);

                            Publisher customerNotificationPublisher = new Publisher(_iconfiguration, notificationConfig.SNSTopic);
                            await customerNotificationPublisher.PublishAsync(notificationMessage);
                            try
                            {
                                DatabaseResponse notificationLogResponse = await _adminOrderDataAccess.CreateEMailNotificationLogForDevPurpose(
                                    new NotificationLogForDevPurpose
                                    {
                                        EventType = NotificationEvent.OrderSuccess.ToString(),
                                        Message = JsonConvert.SerializeObject(notificationMessage)

                                    });

                            }
                            catch (Exception ex)
                            {
                                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            }
                            //Sending message Stop
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = null

                            });
                        }
                        else if (returnResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            var emailDetails = (EmailResponse)returnResponse.Results;
                            DatabaseResponse configResponse = new DatabaseResponse();
                            DatabaseResponse tokenCreationResponse = new DatabaseResponse();
                            string finalURL = string.Empty;
                            if (emailDetails.VerificationStatus == 2) // Rejected then token
                            {
                                configResponse = ConfigHelper.GetValueByKey(ConfigKeys.NRICReUploadLink.GetDescription(), _iconfiguration);
                                tokenCreationResponse = await _adminOrderDataAccess.CreateTokenForVerificationRequests(request.OrderID);
                                var tokenCreation = (VerificationRequestResponse)tokenCreationResponse.Results;
                                finalURL = configResponse.Results.ToString() + tokenCreation.RequestToken;

                                DatabaseResponse registrationResponse = await _adminOrderDataAccess.GetEmailNotificationTemplate(NotificationEvent.ICValidationReject.GetDescription());

                                var notificationMessage = MessageHelper.GetMessage(emailDetails.Email, emailDetails.Name, NotificationEvent.ICValidationReject.GetDescription(),
                               ((EmailTemplate)registrationResponse.Results).TemplateName,
                           _iconfiguration, string.IsNullOrWhiteSpace(finalURL) ? "-" : finalURL, string.IsNullOrWhiteSpace(emailDetails.Remark) ? "-" : emailDetails.Remark, string.IsNullOrWhiteSpace(emailDetails.ChangeLog) ? "-" : emailDetails.ChangeLog);
                                var notificationResponse = await _adminOrderDataAccess.GetConfiguration(ConfiType.Notification.ToString());


                                MiscHelper parser = new MiscHelper();
                                var notificationConfig = parser.GetNotificationConfig((List<Dictionary<string, string>>)notificationResponse.Results);

                                Publisher customerNotificationPublisher = new Publisher(_iconfiguration, notificationConfig.SNSTopic);
                                await customerNotificationPublisher.PublishAsync(notificationMessage);
                                try
                                {
                                    DatabaseResponse notificationLogResponse = await _adminOrderDataAccess.CreateEMailNotificationLogForDevPurpose(
                                        new NotificationLogForDevPurpose
                                        {
                                            EventType = NotificationEvent.OrderSuccess.ToString(),
                                            Message = JsonConvert.SerializeObject(notificationMessage)

                                        });

                                }
                                catch (Exception ex)
                                {
                                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                                }
                            }
                            else
                            {
                                var result = await _commonDataAccess.UpdateTokenForVerificationRequests(request.OrderID);
                            }

                            //Sending message start
                            // Send email to customer email                            ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);


                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = null

                            });
                        }
                        else
                        {
                            LogInfo.Error("UpdateNRICDetails failed for " + request.OrderID + " Order Id " + DbReturnValue.UpdationFailed);
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.UpdationFailed),
                                IsDomainValidationErrors = false
                            });
                        }

                    }
                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }

                }

                else
                {
                    // token auth failure
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = false
                    });
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }

        }

        /// <summary>
        /// Gets the order details history.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="orderID">The order identifier.</param>
        /// <returns></returns>
        [HttpGet("GetOrderDetailsHistoryForNRIC/{orderID}")]
        public async Task<IActionResult> GetOrderDetailsHistory([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int orderID)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (!ModelState.IsValid)
                        {
                            return StatusCode((int)HttpStatusCode.OK,
                                new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = true,
                                    Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                                });
                        }
                        var orderList = await _adminOrderDataAccess.GetNRICOrderDetailsHistory(orderID);

                        if (orderList == null || orderList.Count == 0)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)

                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = orderList

                            });


                        }

                    }
                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }

                }

                else
                {
                    // token auth failure
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = false
                    });
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });

            }

        }

    }
}