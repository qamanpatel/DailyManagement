using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DailyManagementSystem.Services.Interfaces;

namespace DailyManagementSystem.Services.Implementations
{
    public class FilePickerService : IFilePickerService
    {
        private Window? _mainWindow;

        public void Initialize(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public async Task<string?> PickImageAsync()
        {
            if (_mainWindow == null) return null;

            var topLevel = TopLevel.GetTopLevel(_mainWindow);
            if (topLevel == null) return null;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Order Image",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            return files.Count >= 1 ? files[0].Path.LocalPath : null;
        }
    }
}
