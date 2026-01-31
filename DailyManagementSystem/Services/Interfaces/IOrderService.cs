using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DailyManagementSystem.Models;

namespace DailyManagementSystem.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(int orderId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByClientAsync(int clientId);
        Task<IEnumerable<Order>> GetOrdersByMonthAsync(int year, int month);
        Task MarkOrderAsDeliveredAsync(int orderId, DateTime deliveredDate);
        Task<decimal> GetTotalOrderAmountByMonthAsync(int year, int month);
        Task<decimal> GetTotalPendingOrderAmountAsync();
    }
}
