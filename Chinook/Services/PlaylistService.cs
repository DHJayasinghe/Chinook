using Humanizer;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System;

namespace Chinook.Services
{
    public class PlaylistService
    {
        public event EventHandler OnDataUpdated;


        public void AddItem()
        {
            OnEventOccurred();
        }

        protected virtual void OnEventOccurred()
        {
            // Check if there are any subscribers to the event
            EventHandler handler = OnDataUpdated;
            if (handler != null)
            {
                var args = EventArgs.Empty;
                Task.Run(() => handler(this, args));
            }
        }
    }
}
