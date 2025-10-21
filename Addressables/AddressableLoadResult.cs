using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Framework.Addressables
{
    /// <summary>
    /// Represents the result of an Addressables asset loading operation.
    /// Provides a success state, loaded assets as a read-only dictionary, optional error details, and optional
    /// AsyncOperationHandle.
    /// </summary>
    /// <typeparam name="T">The type of assets that were loaded.</typeparam>
    public readonly struct AddressableLoadResult<T> where T : Object
    {
        /// <summary>
        /// Indicates whether the loading operation completed successfully.
        /// </summary>
        public readonly bool Success;

        /// <summary>
        /// A dictionary of all successfully loaded assets, keyed by their asset name.
        /// </summary>
        public readonly IReadOnlyDictionary<string, T> Assets;

        /// <summary>
        /// The Addressables label that was used for the loading operation.
        /// </summary>
        public readonly string Label;

        /// <summary>
        /// The error message if the operation failed or was canceled. Null if successful.
        /// </summary>
        public readonly string ErrorMessage;

        /// <summary>
        /// Optional reference to the underlying Addressables <see cref="AsyncOperationHandle{TObject}" />
        /// used for this loading operation.
        /// Provides the caller with the ability to manually release or track the handle.
        /// Will be null if the loading method released the handle automatically (e.g., <c>autoRelease = true</c> in the loader).
        /// </summary>
        public readonly AsyncOperationHandle<IList<T>>? Handle;

        /// <summary>
        /// Creates a new <see cref="AddressableLoadResult{T}" /> instance.
        /// </summary>
        /// <param name="success">Whether the load operation succeeded.</param>
        /// <param name="label">The label used to load the assets.</param>
        /// <param name="assets">The loaded assets.</param>
        /// <param name="errorMessage">An optional error message in case of failure.</param>
        /// <param name="handle">
        /// Optional reference to the underlying <see cref="AsyncOperationHandle{TObject}" /> used for this operation.
        /// Can be used to manually release or track the handle. Will be null if <c>autoRelease = true</c>.
        /// </param>
        public AddressableLoadResult(bool success, string label, IDictionary<string, T> assets,
            string errorMessage = null, AsyncOperationHandle<IList<T>>? handle = null)
        {
            Success = success;
            Label = label;
            Assets = new ReadOnlyDictionary<string, T>(assets ?? new Dictionary<string, T>());
            ErrorMessage = errorMessage;
            Handle = handle;
        }
    }
}