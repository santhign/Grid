using System;
using Xunit;
using OrderService.Controllers;
using Microsoft.Extensions.Configuration;
using OrderService.DataAccess;
using Core.Models;
using System.IO;

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
        public void GetById_CustomerNotFound_ReturnsNotFoundResult()
        {
            var notFoundResult = _controller.Get("sss", 4);
            Assert.IsType<OperationResponse>(notFoundResult.Result);
        }
    }
}
