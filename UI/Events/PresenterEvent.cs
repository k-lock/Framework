using System;

namespace Framework.UI.Events
{
    /// <summary>
    /// Generic event used to navigate to a specific presenter type.
    /// </summary>
    /// <typeparam name="TPresenter">The type of the target presenter.</typeparam>
    public class PresenterEvent<TPresenter> : IPresenterEvent
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PresenterEvent{TPresenter}" /> with the given payload.
        /// </summary>
        /// <param name="payload">The strongly typed data to pass to the target presenter.</param>
        public PresenterEvent(object[] payload = null)
        {
            Payload = payload;
        }

        /// <summary>
        /// Gets the type of the target presenter for this event.
        /// </summary>
        public Type TargetPresenter => typeof(TPresenter);

        /// <summary>
        /// Gets the payload associated with this event.
        /// </summary>
        public object[] Payload { get; }
    }
}