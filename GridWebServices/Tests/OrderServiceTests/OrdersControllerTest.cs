using System;
using Xunit;
using OrderService.Controllers;
using Microsoft.Extensions.Configuration;
using OrderService.DataAccess;
using Core.Models;
using System.IO;
using System.Threading.Tasks;

namespace OrderServiceTests
{
    public class OrdersControllerTest
    {
        OrdersController _controller;
        IConfiguration _iconfiguration;
        IMessageQueueDataAccess _messageQueueDataAccess;
        IChangeRequestDataAccess _changeRequestDataAccess;

        public OrdersControllerTest()
        {
            _iconfiguration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables()
              .Build();
            _controller = new OrdersController(_iconfiguration, _messageQueueDataAccess, _changeRequestDataAccess);
        }
        [Fact]        
        public async Task GetById_CustomerNotFound_ReturnsNotFoundResult()
        {
            var notFoundResult = await _controller.Get("sss", 4);
            Assert.IsType<OperationResponse>(notFoundResult);
        }
    }
}
