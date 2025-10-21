#if UNITASK_DOTWEEN_SUPPORT

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Framework.Transitions.Base;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that waits for a DOTween tween to complete.
    /// <para>
    /// <b>Important:</b> Requires the UNITASK_DOTWEEN_SUPPORT scripting define symbol to be enabled.
    /// </para>
    /// </summary>
    public class DoTweenTransition : TransitionBase
    {
        private readonly Tween tween;

        /// <summary>
        /// Creates a new transition that waits for the specified tween to complete.
        /// </summary>
        /// <param name="tween">The tween to wait for.</param>
        public DoTweenTransition(Tween tween)
        {
            this.tween = tween;
        }

        /// <summary>
        /// Waits for the tween to complete with cancellation support.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            if (tween == null || !tween.IsActive() || tween.IsComplete())
            {
                return;
            }

            await tween.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
        }
    }
}

#else
#error "DoTweenTransition requires UNITASK_DOTWEEN_SUPPORT scripting define symbol.""

#endif