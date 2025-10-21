using System;

namespace Framework.UI.Events
{
    /// <summary>
    /// Interface for events that trigger navigation between UI presenters.
    /// Implement this to define a specific navigation action from one presenter to another.
    /// </summary>
    public interface IPresenterEvent
    {
        /// <summary>
        /// The type of the target presenter this event should navigate to.
        /// </summary>
        Type TargetPresenter { get; }

        public object[] Payload { get; }
    }
}