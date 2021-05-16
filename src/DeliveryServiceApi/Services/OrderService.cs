using DeliveryServiceApi.Interfaces;

namespace DeliveryServiceApi.Services
{
    public class OrderService : IOrderService
    {
        public bool IsFreeCourierAvailable()
        {
            return true;
        }
    }
}
