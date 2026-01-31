using System.Collections.Generic;
using System.Threading.Tasks;
using DailyManagementSystem.Models;

namespace DailyManagementSystem.Services.Interfaces
{
    public interface IClientService
    {
        Task<Client> CreateClientAsync(Client client);
        Task<Client> UpdateClientAsync(Client client);
        Task DeleteClientAsync(int clientId);
        Task<Client?> GetClientByIdAsync(int clientId);
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<IEnumerable<Client>> GetAllActiveClientsAsync();
        Task<decimal> GetClientOutstandingAmountAsync(int clientId);
        Task<bool> IsClientNameExistsAsync(string clientName);
    }
}
