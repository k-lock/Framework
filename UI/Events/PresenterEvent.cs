using System;

namespace Framework.UI.Events
{
    /// <summary>
    /// Generic event used to navigate to a specific presenter type.
    /// </summary>
    /// <typeparam name="T">The target presenter type to navigate to.</typeparam>
    public class PresenterEvent<T> : IPresenterEvent
    {
        public PresenterEvent(object[] payload = null)
        {
            Payload = payload;
        }

        /// <summary>
        /// The type of the presenter to navigate to.
        /// </summary>
        public Type TargetPresenter => typeof(T);

        public object[] Payload { get; }
    }
}