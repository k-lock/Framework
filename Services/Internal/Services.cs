using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Framework.Services.Internal
{
    /// <summary>
    /// Internal service hub managing registration, retrieval, initialization, and disposal
    /// of all services within the framework. Ensures thread-safe operations.
    /// </summary>
    internal static class Services
    {
        /// <summary>
        /// Internal registry of all currently registered service instances,
        /// keyed by their type.
        /// </summary>
        private static readonly Dictionary<Type, IService> ServiceRegistry = new();

        /// <summary>
        /// Synchronization object used to ensure thread-safe operations
        /// when accessing or modifying the <see cref="ServiceRegistry" />.
        /// </summary>
        private static readonly object Lock = new();

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
        /// Registers a service instance using its concrete type.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if the service is null.</exception>
        internal static void Register(IService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            RegisterInternal(service);
        }

        /// <summary>
        /// Registers a service instance for a specific interface type.
        /// Also registers the concrete type internally.
        /// </summary>
        /// <typeparam name="TInterface">The interface type of the service.</typeparam>
        /// <param name="service">The service instance to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if the service is null.</exception>
        internal static void Register<TInterface>(TInterface service) where TInterface : class, IService
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            lock (Lock)
            {
                Type interfaceType = typeof(TInterface);
                Type concreteType = service.GetType();

                if (interfaceType != concreteType)
                {
                    ServiceRegistry.TryAdd(interfaceType, service);
                }
            }

            RegisterInternal(service);
        }

        /// <summary>
        /// Internal registration logic for a service instance.
        /// Adds the service to the registry and calls its OnRegister lifecycle method.
        /// </summary>
        private static void RegisterInternal(IService service)
        {
            lock (Lock)
            {
                Type type = service.GetType();
                if (!ServiceRegistry.TryAdd(type, service))
                {
                    Debug.Log($"Service of type {type} is already registered.");
                    return;
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
        }

        /// <summary>
        /// Retrieves a registered service by its type.
        /// </summary>
        /// <typeparam name="T">The interface or concrete type of the service.</typeparam>
        /// <returns>The registered service instance, or null if not found.</returns>
        internal static T Get<T>() where T : class, IService
        {
            lock (Lock)
            {
                if (ServiceRegistry.TryGetValue(typeof(T), out var service))
                {
                    return service as T;
                }
            }

            Debug.Log($"Service of type {typeof(T).Name} is not registered.");

            return null;
        }

        /// <summary>
        /// Disposes all registered services and clears the internal registry.
        /// </summary>
        internal static void Dispose()
        {
            lock (Lock)
            {
                foreach (var service in ServiceRegistry.Values)
                {
                    service.Dispose();
                }

                ServiceRegistry.Clear();
            }
        }

        /// <summary>
        /// Asynchronously initializes all registered services by calling
        /// OnInitializeAsync followed by OnInitializeComplete for each service.
        /// Errors in individual services are logged but do not prevent others from initializing.
        /// </summary>
        internal static async UniTask InitializeAllAsync()
        {
            HashSet<IService> services;
            lock (Lock)
            {
                services = new HashSet<IService>(ServiceRegistry.Values);
            }

            foreach (var service in services)
            {
                try
                {
                    await service.OnInitializeAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in OnInitializeAsync for {service.GetType().Name}: {ex}");
                }
            }

            foreach (var service in services)
            {
                try
                {
                    service.OnInitializeComplete();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in OnInitializeComplete for {service.GetType().Name}: {ex}");
                }
            }
        }

        /// <summary>
        /// Returns all currently registered services as a read-only collection.
        /// </summary>
        /// <returns>A read-only collection of all registered services.</returns>
        internal static IReadOnlyCollection<IService> GetAllRegisteredServices()
        {
            lock (Lock)
            {
                return ServiceRegistry.Values;
            }
        }
    }
}