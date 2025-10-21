using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Framework.Services.Internal
{
    /// <summary>
    /// Internal service hub managing service registration and retrieval.
    /// </summary>
    internal static class Services
    {
        private static readonly Dictionary<Type, IService> ServiceRegistry = new();

#if UNITY_EDITOR
        /// <summary>
        /// Clears all registered services on Playmode start to prevent stale references.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetServices()
        {
            Dispose();
        }
#endif

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        internal static void Register<T>(T service) where T : class, IService
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Type type = typeof(T);
            if (!ServiceRegistry.TryAdd(type, service))
            {
                throw new Exception($"Service of type {type} is already registered.");
            }

            try
            {
                service.OnRegister();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in OnRegister for {type.Name}: {ex}");
            }
        }

        /// <summary>
        /// Retrieves a registered service instance.
        /// </summary>
        internal static T Get<T>() where T : class, IService
        {
            ServiceRegistry.TryGetValue(typeof(T), out var service);
            return service as T;
        }

        /// <summary>
        /// Clears all registered services and dispose them.
        /// </summary>
        internal static void Dispose()
        {
            foreach (var service in ServiceRegistry.Values)
            {
                service.Dispose();
            }

            ServiceRegistry.Clear();
        }

        /// <summary>
        /// Initializes all registered services in order.
        /// Calls OnInitializeAsync and then OnInitializeComplete on each service.
        /// </summary>
        internal static async UniTask InitializeAllAsync()
        {
            foreach (var service in ServiceRegistry.Values)
            {
                await service.OnInitializeAsync();
                service.OnInitializeComplete();
            }
        }

        /// <summary>
        /// Returns all registered services.
        /// </summary>
        public static IReadOnlyCollection<IService> GetAllRegisteredServices()
        {
            return ServiceRegistry.Values;
        }
    }
}