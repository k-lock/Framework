using DG.Tweening;
using Framework.Transitions.Base;
using Framework.Transitions.Implementations;

namespace Framework.Transitions.Extensions
{
    /// <summary>
    /// Extension methods for DOTween integration with the transition system.
    /// </summary>
    public static class TweenTransitionExtensions
    {
        /// <summary>
        /// Waits for the tween to complete.
        /// </summary>
        /// <param name="tween">The tween to wait for.</param>
        /// <returns>A transition that completes when the tween completes.</returns>
        public static ITransition WaitForComplete(this Tween tween)
        {
            return new DoTweenTransition(tween);
        }
    }
}