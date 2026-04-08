using System;
using System.Collections.Generic;

namespace App
{
    public static class ServiceContainer
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<TService>(TService service) where TService : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Services[typeof(TService)] = service;
        }

        public static TService Resolve<TService>() where TService : class
        {
            if (TryResolve<TService>(out TService service))
            {
                return service;
            }

            throw new InvalidOperationException("Service of type '" + typeof(TService).FullName + "' is not registered.");
        }

        public static bool TryResolve<TService>(out TService service) where TService : class
        {
            if (Services.TryGetValue(typeof(TService), out object resolvedService))
            {
                service = resolvedService as TService;
                return service != null;
            }

            service = null;
            return false;
        }

        public static void Reset()
        {
            Services.Clear();
        }
    }
}
