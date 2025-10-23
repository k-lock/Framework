namespace Framework.Services
{
    /// <summary>
    /// Cached wrapper for a service.
    /// Provides a single access point to the service instance.
    /// </summary>
    /// <typeparam name="TService">Type of the service.</typeparam>
    public class Service<TService> where TService : class, IService
    {
        private TService value;

        /// <summary>
        /// Gets the cached service instance.
        /// Automatically retrieves it from the service hub on first access.
        /// </summary>
        private TService Value => value ??= Internal.Services.Get<TService>();

        /// <summary>
        /// Clears the cached service instance.
        /// </summary>
        public void ClearCache()
        {
            value = null;
        }

        public static implicit operator TService(Service<TService> service)
        {
            return service.Value;
        }
    }
}