using System;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Animation
{
    /// <summary>
    /// Per-item content appearance animation (typewriter, fade, instant, …).
    /// Orchestration (sequential play / global skip) lives outside this interface.
    /// </summary>
    public interface IContentAnimator
    {
        bool IsPlaying { get; }

        /// <summary>
        /// Raised once when the animation reaches its final visual state
        /// (either naturally or after <see cref="Skip"/>).
        /// </summary>
        event Action Completed;

        /// <summary>
        /// Start animation from the item's initial visual state toward the final state.
        /// No-op if already complete. If already playing — ignore a second call.
        /// </summary>
        void Play();

        /// <summary>
        /// Immediately apply the final visual state and raise <see cref="Completed"/>
        /// (if not already completed). Safe to call when not playing.
        /// </summary>
        void Skip();
    }
}
