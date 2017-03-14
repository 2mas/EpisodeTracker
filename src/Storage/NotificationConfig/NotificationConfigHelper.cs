using System;
using System.Collections.Generic;
using System.Linq;

namespace EpisodeTracker.Storage.NotificationConfig
{
    public static class NotificationConfigHelper
    {
        public static List<Type> GetINotifierImplementationTypes()
        {
            var notifierConfigInterface = typeof(INotifierConfiguration);
            var implementations = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => notifierConfigInterface.IsAssignableFrom(p) && !p.IsInterface).ToList();

            return implementations;        
        }
    }
}
