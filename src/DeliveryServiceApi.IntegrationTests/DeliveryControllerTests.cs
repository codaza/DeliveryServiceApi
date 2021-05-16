using DeliveryServiceApi.Data;
using DeliveryServiceApi.Data.Models;
using DeliveryServiceApi.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeliveryServiceApi.IntegrationTests
{
    [TestFixture]
    public class DeliveryControllerTests
    {
        [Test]
        public async Task CheckStatus_SendRequest_ShouldReturnOk()
        {
            // Arrange

            WebApplicationFactory<Startup> webHost = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var dbContextDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    services.Remove(dbContextDescriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("delivery_db");
                    });
                });
            });

            HttpClient httpClient = webHost.CreateClient();

            // Act

            HttpResponseMessage response = await httpClient.GetAsync("api/delivery/check-status");

            // Assert

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task SendOrder_FreeCourierAvailable_ShouldReturnNotFound()
        {
            // Arrange

            WebApplicationFactory<Startup> webHost = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var dbContextDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    services.Remove(dbContextDescriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("delivery_db");
                    });

                    var orderService = services.SingleOrDefault(d => d.ServiceType == typeof(IOrderService));

                    services.Remove(orderService);

                    var mockService = new Mock<IOrderService>();

                    mockService.Setup(_ => _.IsFreeCourierAvailable()).Returns(() => false);

                    services.AddTransient(_ => mockService.Object);
                });
            });

            HttpClient httpClient = webHost.CreateClient();

            // Act

            HttpResponseMessage response = await httpClient.PostAsync("api/delivery/send-order", null);

            // Assert

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetOrdersCount_SendRequest_ShouldReturnActualOrdersCount()
        {
            // Arrange

            WebApplicationFactory<Startup> webHost = new WebApplicationFactory<Startup>().WithWebHostBuilder(builer =>
            {
                builer.ConfigureTestServices(services =>
                {
                    var dbContextDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    services.Remove(dbContextDescriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("delivery_db");
                    });
                });
            });

            ApplicationDbContext dbContext =
                webHost.Services.CreateScope().ServiceProvider.GetService<ApplicationDbContext>();

            List<Order> orders = new() { new Order(), new Order(), new Order() };

            await dbContext.Orders.AddRangeAsync(orders);

            await dbContext.SaveChangesAsync();

            HttpClient httpClient = webHost.CreateClient();

            // Act

            HttpResponseMessage response = await httpClient.GetAsync("api/delivery/orders-count");

            // Assert

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            int ordersCount = int.Parse(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(orders.Count, ordersCount);
        }
    }
}
