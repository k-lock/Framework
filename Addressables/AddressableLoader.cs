using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Framework.Addressables
{
    /// <summary>
    /// Utility class that provides asynchronous loading of Addressable assets using UniTask.
    /// Supports progress reporting, cancellation, error handling, and automatic handle release.
    /// </summary>
    public static class AddressableLoader
    {
        /// <summary>
        /// Asynchronously loads all Addressable assets associated with a specific label.
        /// Returns a read-only dictionary of the loaded assets wrapped in an <see cref="AddressableLoadResult{TObject}"/>.
        /// </summary>
        /// <typeparam name="TObject">The type of assets to load.</typeparam>
        /// <param name="label">The Addressables label used to locate the assets.</param>
        /// <param name="progress">Optional progress reporter (0.0–1.0).</param>
        /// <param name="onAssetLoaded">Optional callback invoked for each asset as soon as it is loaded.</param>
        /// <param name="cancellationToken">Optional cancellation token to abort the loading process.</param>
        /// <param name="autoRelease">
        /// If true, the Addressables operation handle will be released automatically after loading.
        /// Use with care — you must keep references to the assets if you intend to use them later.
        /// </param>
        /// <returns>
        /// An <see cref="AddressableLoadResult{TObject}"/> containing the status of the operation and any loaded assets.
        /// </returns>
        public static async UniTask<AddressableLoadResult<TObject>> LoadAssetsByLabelAsync<TObject>
        (
            string label,
            IProgress<float> progress = null,
            Action<TObject> onAssetLoaded = null,
            CancellationToken cancellationToken = default,
            bool autoRelease = false
        ) where TObject : Object
        {
            IDictionary<string, TObject> assetsDict = new Dictionary<string, TObject>();

            if (string.IsNullOrWhiteSpace(label))
            {
                string error = "[Addressables] ❌ Label is null or empty.";
                Debug.LogError(error);
                return new AddressableLoadResult<TObject>(false, label, assetsDict, error);
            }

            AsyncOperationHandle<IList<TObject>> handle;
            try
            {
                handle = UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<TObject>(
                    label,
                    asset =>
                    {
                        if (asset == null)
                        {
                            return;
                        }

                        string key = asset.name;
                        int counter = 1;

                        while (assetsDict.ContainsKey(key))
                        {
                            key = $"{asset.name}#{counter++}";
                        }

                        assetsDict[key] = asset;
                        onAssetLoaded?.Invoke(asset);
                    }
                );
            }
            catch (Exception ex)
            {
                string error = $"[Addressables] ❌ Exception while starting load for label '{label}': {ex}";
                Debug.LogError(error);
                return new AddressableLoadResult<TObject>(false, label, assetsDict, error);
            }

            try
            {
                while (!handle.IsDone)
                {
                    progress?.Report(handle.PercentComplete);
                    await UniTask.Yield(cancellationToken);
                }

                await handle.ToUniTask(cancellationToken: cancellationToken);

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    var opEx = handle.IsValid() && handle.OperationException != null
                        ? $"\nOperationException:\n{handle.OperationException}"
                        : string.Empty;

                    string error =
                        $"[Addressables] ❌ Loading assets with label '{label}' failed (Status={handle.Status}).{opEx}";
                    Debug.LogError(error);
                    return new AddressableLoadResult<TObject>
                    (
                        false,
                        label,
                        assetsDict,
                        error,
                        autoRelease ? null : handle
                    );
                }

                progress?.Report(1f);
                return new AddressableLoadResult<TObject>
                (
                    true,
                    label,
                    assetsDict,
                    handle: autoRelease ? null : handle
                );
            }
            catch (OperationCanceledException)
            {
                string warning = $"[Addressables] ⚠ Loading cancelled for label '{label}'.";
                Debug.LogWarning(warning);
                return new AddressableLoadResult<TObject>
                (
                    false,
                    label,
                    assetsDict,
                    warning,
                    autoRelease ? null : handle
                );
            }
            catch (Exception ex)
            {
                var opEx2 = handle.IsValid() && handle.OperationException != null
                    ? $"\nOperationException:\n{handle.OperationException}"
                    : string.Empty;

                string error = $"[Addressables] ❌ Exception while loading label '{label}': {ex}{opEx2}";
                Debug.LogError(error);
                return new AddressableLoadResult<TObject>
                (
                    false,
                    label,
                    assetsDict,
                    error,
                    autoRelease ? null : handle
                );
            }
            finally
            {
                if (autoRelease && handle.IsValid())
                {
                    try
                    {
                        UnityEngine.AddressableAssets.Addressables.Release(handle);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[Addressables] ⚠ Failed to release handle for label '{label}': {ex}");
                    }
                }
            }
        }
    }
}