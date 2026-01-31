using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DailyManagementSystem.Data;
using DailyManagementSystem.Models;
using DailyManagementSystem.Services.Interfaces;

namespace DailyManagementSystem.Services.Implementations
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;

        public ClientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            if (await IsClientNameExistsAsync(client.ClientName))
                throw new InvalidOperationException($"Client with name '{client.ClientName}' already exists.");

            client.IsActive = true;
            client.CreatedAt = DateTime.Now;
            
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<Client> UpdateClientAsync(Client client)
        {
            var existingClient = await _context.Clients.FindAsync(client.ClientId);
            if (existingClient == null)
                throw new KeyNotFoundException($"Client with ID {client.ClientId} not found.");

            existingClient.ClientName = client.ClientName;
            existingClient.Phone = client.Phone;
            existingClient.Address = client.Address;
            existingClient.IsActive = client.IsActive;
            existingClient.UpdatedAt = DateTime.Now;

            _context.Clients.Update(existingClient);
            await _context.SaveChangesAsync();
            return existingClient;
        }

        public async Task DeleteClientAsync(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
                throw new KeyNotFoundException($"Client with ID {clientId} not found.");

            // Soft delete
            client.IsActive = false;
            client.UpdatedAt = DateTime.Now;
            
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task<Client?> GetClientByIdAsync(int clientId)
        {
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _context.Clients
                .AsNoTracking()
                .OrderBy(c => c.ClientName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Client>> GetAllActiveClientsAsync()
        {
            return await _context.Clients
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.ClientName)
                .ToListAsync();
        }

        public async Task<decimal> GetClientOutstandingAmountAsync(int clientId)
        {
            var totalOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.ClientId == clientId)
                .SumAsync(o => o.OrderAmount);

            var totalPayments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.ClientId == clientId)
                .SumAsync(p => p.AmountReceived);

            return totalOrders - totalPayments;
        }

        public async Task<bool> IsClientNameExistsAsync(string clientName)
        {
            return await _context.Clients
                .AnyAsync(c => c.ClientName.ToLower() == clientName.ToLower() && c.IsActive);
        }
    }
}
