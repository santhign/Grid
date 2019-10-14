using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CustomerService.Models;
using CustomerService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Helpers;
using Core.Models;
using Core.Enums;
using System.Threading.Tasks;
using Serilog;

namespace CustomerService.Services
{
    public interface IUserService
    {
        Task<Customer> Authenticate(LoginDto loginDetails);        
    }

    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        internal DataAccessHelper _DataHelper = null;
        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }        
        
        public async Task<Customer> Authenticate(LoginDto loginDetails)
        {
            var customer = new Customer();
            AccountDataAccess _AccountAccess;
            DatabaseResponse response;
            try
            {
                _AccountAccess = new AccountDataAccess(_configuration);
                response = await _AccountAccess.AuthenticateCustomer(loginDetails);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Exception in calling AuthenticateCustomer. {function}", "Authenticate");
                return null;
            }

            if (response.ResponseCode == ((int)DbReturnValue.AuthSuccess))
            {
                try
                {
                    // authentication successful so generate jwt token                
                    customer = (Customer)response.Results;
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes("stratagile grid customer signin jwt hashing secret");
                    DatabaseResponse configResponse = ConfigHelper.GetValueByKey(ConfigKeys.CustomerTokenExpiryInDays.ToString(), _configuration);
                    int expiry = 0;
                    if (configResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                    {
                        expiry = int.Parse(configResponse.Results.ToString());
                    }
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                            {
                             new Claim(ClaimTypes.Name, customer.CustomerID.ToString())
                            }),

                        Expires = DateTime.Now.AddDays(expiry),

                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);
                    DatabaseResponse tokenResponse = new DatabaseResponse();
                    tokenResponse = await _AccountAccess.LogCustomerToken(customer.CustomerID, tokenString);
                    customer.Token = tokenString;
                    return customer;
                }
                catch(Exception ex)
                {
                    Log.Error(ex, "Exception in generating jwt token. {function}", "Authenticate");
                    return null;
                }
            }
            else
            {
                //Authentication failed
                return null;
            }            
        }
       
    }
}