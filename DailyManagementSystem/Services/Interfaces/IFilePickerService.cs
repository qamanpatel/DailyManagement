using System.Threading.Tasks;

namespace DailyManagementSystem.Services.Interfaces
{
    public interface IFilePickerService
    {
        Task<string?> PickImageAsync();
    }
}
