using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CustomerService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using CustomerService.DataAccess;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using InfrastructureService;


namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public AccountController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }
        /// <summary>
        /// Authenticate customer against Email and Password given.
        /// Returns logged in Principle with success status, auth token and logged customer details
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns>LoggedInPrinciple</returns>
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]LoginDto loginRequest)
        {
            try
            {
                
                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
                }
               

                AccountDataAccess _AccountAccess = new AccountDataAccess(_iconfiguration);

                DatabaseResponse response = await _AccountAccess.AuthenticateCustomer(loginRequest);

                if (response.ResponseCode == ((int)DbReturnValue.EmailNotExists))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.EmailNotExists),
                        IsDomainValidationErrors = true
                    });
                }
                else if (response.ResponseCode == ((int)DbReturnValue.PasswordIncorrect))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.PasswordIncorrect),
                        IsDomainValidationErrors = true
                    });
                }

                else if(response.ResponseCode == ((int)DbReturnValue.AuthSuccess))
                {
                    //Authentication success

                    var customer = new Customer();

                    customer = (Customer) response.Results;

                    var tokenHandler = new JwtSecurityTokenHandler();

                    var key = Encoding.ASCII.GetBytes("stratagile grid customer signin jwt hashing secret");

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                             new Claim(ClaimTypes.Name, customer.CustomerID.ToString())
                        }),
                        Expires = DateTime.UtcNow.AddDays(7), //  need to check with business needs
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    var tokenString = tokenHandler.WriteToken(token);

                    DatabaseResponse tokenResponse = new DatabaseResponse();

                    tokenResponse=await _AccountAccess.LogCustomerToken(tokenString);

                    // return basic user info (without password) and token to store client side
                    return Ok(new OperationResponse
                    {     HasSucceeded=true,
                          Message=EnumExtensions.GetDescription(DbReturnValue.AuthSuccess),
                           ReturnedObject = new LoggedInPrinciple
                           {
                               Customer = customer,
                               IsAuthenticated = true,
                               Token = tokenString
                           }
                    }                  
                    );
                }

                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.ReasonUnknown),
                        IsDomainValidationErrors = true
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