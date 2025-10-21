using System;
using Cysharp.Threading.Tasks;

namespace Framework.Services
{
    /// <summary>
    /// Base interface for all services.
    /// </summary>
    public interface IService : IDisposable
    {
        /// <summary>
        /// Called when the service is registered in the Services hub.
        /// </summary>
        void OnRegister();

        /// <summary>
        /// Called to initialize the service asynchronously.
        /// </summary>
        UniTask OnInitializeAsync();

        /// <summary>
        /// Called when all initializations are complete.
        /// </summary>
        void OnInitializeComplete();
    }
}