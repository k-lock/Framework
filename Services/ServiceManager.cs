using Cysharp.Threading.Tasks;

namespace Framework.Services
{
    /// <summary>
    /// Facade class providing centralized access to all services within the framework.
    /// Simplifies retrieval, registration, initialization, and disposal of services.
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>
        /// Retrieves a registered service by its type.
        /// </summary>
        /// <typeparam name="T">The interface or concrete type of the service.</typeparam>
        /// <returns>The registered service instance, or null if not found.</returns>
        public static T Get<T>() where T : class, IService
        {
            return Internal.Services.Get<T>();
        }

        /// <summary>
        /// Registers a service instance using its concrete type.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        public static void Register(IService service)
        {
            Internal.Services.Register(service);
        }

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        /// <typeparam name="TInterface">The type of service being registered.</typeparam>
        /// <param name="service">The service instance to register.</param>
        public static void Register<TInterface>(TInterface service) where TInterface : class, IService
        {
            Internal.Services.Register(service);
        }

        /// <summary>
        /// Initializes all registered services in order.
        /// Calls OnInitializeAsync and then OnInitializeComplete on each service.
        /// </summary>
        public static UniTask InitializeAllAsync()
        {
            return Internal.Services.InitializeAllAsync();
        }

        /// <summary>
        /// Disposes all registered services and releases associated resources.
        /// </summary>
        public static void Dispose()
        {
            Internal.Services.Dispose();
        }
    }
}