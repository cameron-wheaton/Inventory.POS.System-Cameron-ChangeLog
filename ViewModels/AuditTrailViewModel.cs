using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INVApp.Models;
using INVApp.Services;

namespace INVApp.ViewModels
{
    public class AuditTrailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private int _productId;

        public ObservableCollection<InventoryLog> InventoryLogs { get; } = new ObservableCollection<InventoryLog>();

        public AuditTrailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task LoadLogsAsync(int productId)
        {
            _productId = productId;
            var logs = await _databaseService.GetInventoryLogsAsync(productId);
            InventoryLogs.Clear();

            foreach (var log in logs)
            {
                InventoryLogs.Add(log);
            }
        }

    }
}
