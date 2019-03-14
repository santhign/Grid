using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerService.Contexts;
using CustomerService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly CustomerContext _context;

        public AccountController(CustomerContext context)
        {
            _context = context;
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]LoginDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                return BadRequest(messages);
            }
            var customer = await _context.Customers.FromSql($"[Customer_AuthenticateCustomer] @Email={loginRequest.Email}, @Password={new Sha2().Hash(loginRequest.Password)}").FirstOrDefaultAsync();

            if (customer == null)
            {
                customer = await _context.Customers.FromSql($"[Customer_GetCustomerByEmail] @Email={loginRequest.Email}").FirstOrDefaultAsync();

                if (customer == null)
                {
                    return BadRequest("Email does not exists");
                }

                else
                {
                    return BadRequest("Incorrect password");
                }
            }
            else
            {
                //Authentication success

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

                // return basic user info (without password) and token to store client side
                return Ok(new LoggedInPrinciple
                {
                    Customer = customer,
                    IsAuthenticated = true,
                    Token = tokenString
                });
            }
        }
    }
}