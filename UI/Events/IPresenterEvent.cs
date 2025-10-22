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
        /// Gets the type of the target presenter that this event is intended for.
        /// </summary>
        Type TargetPresenter { get; }

        /// <summary>
        /// Gets the strongly typed payload for this event.
        /// </summary>
        object[] Payload { get; }
    }
}