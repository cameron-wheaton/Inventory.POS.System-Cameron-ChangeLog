using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Services
{
    public class NotificationService
    {
        public delegate void NotifyEventHandler(string message);
        public event NotifyEventHandler OnNotify;

        public void Notify(string message)
        {
            OnNotify?.Invoke(message); 
        }
    }
}
