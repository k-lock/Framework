using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Observable;
using Framework.UI.Model;
using UnityEngine.UIElements;

namespace Framework.UI.Presenter
{
    /// <summary>
    /// Base class for all UI presenters.
    /// Provides lifecycle management, callback registration, and observable events for UI elements.
    /// </summary>
    /// <typeparam name="TPresenterEvent">Type of events that the presenter publishes.</typeparam>
    /// <typeparam name="TPresenterModel">Type of the presenter model.</typeparam>
    public abstract class Presenter<TPresenterEvent, TPresenterModel> : IPresenter, IPresenterService<TPresenterModel>
        where TPresenterEvent : class
        where TPresenterModel : IPresenterModel
    {
        /// <summary>
        /// Holds all disposable resources, including event callbacks, for automatic cleanup.
        /// </summary>
        private readonly CompositeDisposable disposables = new();

        /// <summary>
        /// Subject that publishes events of type <typeparamref name="TPresenterEvent" /> to subscribers.
        /// </summary>
        private readonly Subject<TPresenterEvent> events = new();

        /// <summary>
        /// Cancellation token source for managing async operations lifecycle.
        /// Automatically canceled when the presenter is disposed of.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Indicates whether a Show or Hide transition is currently running.
        /// </summary>
        private bool isTransitioning;

        /// <summary>
        /// Holds the next scheduled transition that will be executed after the current transition completes.
        /// </summary>
        private Func<UniTask> queuedTransition;

        /// <summary>
        /// The root element of the UI hierarchy this presenter belongs to.
        /// </summary>
        protected VisualElement RootElement;

        /// <summary>
        /// Observable stream of events emitted by this presenter.
        /// Subscribers can react to actions or navigation requests.
        /// </summary>
        public Observable.IObservable<TPresenterEvent> Events => events;

        /// <summary>
        /// The name of the root element inside the UI hierarchy.
        /// Derived classes must be implemented to locate their main container.
        /// </summary>
        protected abstract string RootElementName { get; }

        /// <summary>
        /// Disposes all subscriptions and resources associated with this presenter.
        /// Cancels all ongoing async operations and cleans up resources.
        /// </summary>
        public virtual void Dispose()
        {
            CancelAndDisposeCancellations();

            isTransitioning = false;
            queuedTransition = null;

            events?.Dispose();
            disposables?.Dispose();
        }

        /// <summary>
        /// Shows the presenter asynchronously, optionally with a transition animation.
        /// If a transition is already running, this call is queued to run afterward.
        /// If the presenter is already visible, this method returns immediately.
        /// </summary>
        /// <returns>A UniTask that completes when the show transition finishes or is canceled.</returns>
        public async UniTask ShowAsync()
        {
            await TransitionAsync(true);
        }

        /// <summary>
        /// Shows the presenter asynchronously, optionally with a transition animation.
        /// If a transition is already running, this call is queued to run afterward.
        /// If the presenter is already visible, this method returns immediately.
        /// </summary>
        /// <returns>A UniTask that completes when the show transition finishes or is canceled.</returns>
        public async UniTask HideAsync()
        {
            await TransitionAsync(false);
        }

        /// <summary>
        /// Override to handle payload data passed to this presenter.
        /// </summary>
        /// <param name="payload"></param>
        public virtual void SetData(object[] payload)
        {
            // Optional override in derived classes to handle payload data.
        }

        /// <summary>
        /// Indicates whether the presenter is currently visible or hidden.
        /// Determined based on the Display and Visibility styles.
        /// </summary>
        public bool IsVisible => PresenterRoot?.resolvedStyle.display != DisplayStyle.None &&
                                 PresenterRoot?.resolvedStyle.visibility == Visibility.Visible;

        /// <summary>
        /// Cancellation token that is canceled when the presenter is disposed of.
        /// Use this token for async operations that should be canceled at the presenter disposal.
        /// </summary>
        public CancellationToken CancellationToken => cancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>
        /// Root container element for this presenter.
        /// </summary>
        public VisualElement PresenterRoot { get; protected set; }

        /// <summary>
        /// The model associated with this presenter, providing data and state.
        /// </summary>
        public TPresenterModel Model { get; private set; }

        /// <summary>
        /// Registers a callback on a VisualElement and ensures automatic disposal.
        /// </summary>
        /// <typeparam name="T">Type of the UIElements event.</typeparam>
        /// <param name="element">The element to attach the callback to.</param>
        /// <param name="callback">The callback delegate.</param>
        public void RegisterCallback<T>(VisualElement element, EventCallback<T> callback)
            where T : EventBase<T>, new()
        {
            element.RegisterCallback(callback);
            AddDisposable(new DisposablePresenterCallback<T>(element, callback));
        }

        public void AddDisposable(IDisposable disposable)
        {
            disposables.Add(disposable);
        }

        /// <summary>
        /// Initializes the presenter with the given root element and optional model.
        /// Sets up callbacks and hides the presenter initially.
        /// </summary>
        /// <param name="root">Root VisualElement of the UI document.</param>
        /// <param name="model">Optional presenter model instance.</param>
        public void Initialize(VisualElement root, TPresenterModel model = default)
        {
            cancellationTokenSource = new CancellationTokenSource();
            Model = model;

            if (Model is IRequiresPresenterService<TPresenterModel> modelWithPresenter)
            {
                modelWithPresenter.Presenter = this;
            }

            PresenterRoot = root ?? throw new ArgumentException("'root' cannot be null");
            RootElement = root.Q<VisualElement>(RootElementName) ??
                          throw new ArgumentException($"'{RootElementName}' not found in UI document");

            Setup();
            HideImmediately();
        }

        /// <summary>
        /// Abstract method for presenter-specific setup logic.
        /// </summary>
        protected abstract void Setup();

        /// <summary>
        /// Publishes an event to all subscribers.
        /// </summary>
        /// <param name="e">The event instance.</param>
        public void Publish(TPresenterEvent e)
        {
            events?.Invoke(e);
        }

        /// <summary>
        /// Called at the very beginning of the show sequence, before visibility changes or transitions.
        /// Override to add custom logic at the start of showing the presenter.
        /// </summary>
        protected virtual void OnShowBegin()
        {
        }

        /// <summary>
        /// Called after the show sequence and any transitions have completed.
        /// Override to add custom logic after the presenter is fully visible.
        /// </summary>
        protected virtual void OnShowComplete()
        {
        }

        /// <summary>
        /// Called at the very beginning of the hide sequence, before visibility changes or transitions.
        /// Override to add custom logic at the start of hiding the presenter.
        /// </summary>
        protected virtual void OnHideBegin()
        {
        }

        /// <summary>
        /// Called after the hide sequence and any transitions have completed.
        /// Override to add custom logic after the presenter is fully hidden.
        /// </summary>
        protected virtual void OnHideComplete()
        {
        }

        private void HideImmediately()
        {
            OnHideBegin();
            SetPresenterVisibility(false);
            OnHideComplete();
        }

        /// <summary>
        /// Override to provide an observable that completes when the show transition ends.
        /// </summary>
        protected virtual Observable.IObservable<TransitionEndEvent> ShowCompletion() => null;


        /// <summary>
        /// Override to provide an observable that completes when the hide transition ends.
        /// </summary>
        protected virtual Observable.IObservable<TransitionEndEvent> HideCompletion() => null;


        /// <summary>
        /// Sets presenter visibility directly without invoking transitions.
        /// </summary>
        /// <param name="visible">Whether the presenter should be visible.</param>
        private void SetPresenterVisibility(bool visible = true)
        {
            PresenterRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            PresenterRoot.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Asynchronously shows or hides the presenter with optional transitions and lifecycle callbacks.
        /// </summary>
        /// <param name="show">If <c>true</c>, the presenter will be shown; if <c>false</c>, it will be hidden.</param>
        /// <remarks>
        /// If a transition is already in progress, this call is queued and executed immediately after the current transition
        /// completes.
        /// The method invokes lifecycle hooks: <see cref="OnShowBegin" />, <see cref="OnShowComplete" />,
        /// <see cref="OnHideBegin" />, and <see cref="OnHideComplete" /> at the appropriate stages.
        /// It also waits for the transition returned by <see cref="ShowCompletion" /> or
        /// <see cref="HideCompletion" /> to complete, allowing for animated transitions.
        /// </remarks>
        private async UniTask TransitionAsync(bool show)
        {
            if (isTransitioning)
            {
                queuedTransition = show ? ShowAsync : HideAsync;
                return;
            }

            if (IsVisible == show)
            {
                return;
            }

            await ScheduleTransitionAsync(async ct =>
                {
                    if (show)
                    {
                        OnShowBegin();
                        SetPresenterVisibility();
                    }
                    else
                    {
                        OnHideBegin();
                    }

                    Observable.IObservable<TransitionEndEvent> observable =
                        show ? ShowCompletion() : HideCompletion();
                    await WaitForTransitionAsync(observable, ct);

                    if (show)
                    {
                        OnShowComplete();
                    }
                    else
                    {
                        SetPresenterVisibility(false);
                        OnHideComplete();
                    }
                }
            );
        }

        /// <summary>
        /// Awaits the completion of a UI transition observable.
        /// If the observable is null, yields for a single frame to allow async flow to continue.
        /// This method respects the provided cancellation token and will stop silently if canceled.
        /// </summary>
        /// <param name="observable">The transition observable to await.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the wait operation.</param>
        /// <returns>A UniTask that completes when the observable signals completion or when canceled.</returns>
        private static async UniTask WaitForTransitionAsync(Observable.IObservable<TransitionEndEvent> observable,
            CancellationToken cancellationToken = default)
        {
            if (observable == null)
            {
                await UniTask.Yield(cancellationToken);
                return;
            }

            UniTaskCompletionSource taskCompletionSource = new();
            using IDisposable disposable = observable.Subscribe(_ => taskCompletionSource.TrySetResult());

            await UniTask.WhenAny
            (
                taskCompletionSource.Task,
                UniTask.WaitUntilCanceled(cancellationToken)
            );
        }

        /// <summary>
        /// Executes a Show or Hide transition sequentially.
        /// If a transition is already in progress, the new transition is queued and executed afterward.
        /// </summary>
        /// <param name="transition">The transition function that accepts a CancellationToken.</param>
        private async UniTask ScheduleTransitionAsync(Func<CancellationToken, UniTask> transition)
        {
            isTransitioning = true;

            try
            {
                await transition(CancellationToken);
            }
            finally
            {
                isTransitioning = false;
                if (queuedTransition != null)
                {
                    Func<UniTask> next = queuedTransition;
                    queuedTransition = null;
                    await next();
                }
            }
        }

        /// <summary>
        /// Cancels and disposes the cancellation token source.
        /// Safely handles cases where the token source is already disposed of.
        /// </summary>
        private void CancelAndDisposeCancellations()
        {
            // Cancel any ongoing async operations
            try
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // Token source was already disposed, ignore
            }
            finally
            {
                cancellationTokenSource = null;
            }
        }
    }
}