using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// The base class for all game components, adds a countdown system
    /// 
    /// #### Countdowns
    /// 
    /// Countdowns are timers used to execute a callback function after a certain amount of ticks.
    /// Countdowns are identifed by a numerical id, subclasses can define an enum of countdown names for this purpose.
    /// 
    /// A countdown is created with the <see cref="DiamondHollow.DHGameComponent.CreateCountdown"/> method and can be used in a few ways:
    /// - Countdowns can have a callback registered, which is called when the countdown is finished, additional callbacks can be added with <see cref="DiamondHollow.DHGameComponent.AddCountdownCallback"/>.
    /// - The state of countdowns can be checked with <see cref="DiamondHollow.DHGameComponent.IsCountdownDone"/> and <see cref="DiamondHollow.DHGameComponent.GetCountdownProgress"/> methods.
    /// - Countdowns can automatically repeat or be reset manually with the <see cref="DiamondHollow.DHGameComponent.ResetCountdown"/> method.
    /// </summary>
    public class DHGameComponent : DrawableGameComponent
    {
        /// <summary>
        /// The <see cref="DiamondHollow.DiamondHollowGame"/> this component is attached to.
        /// Overrides the <see cref="Microsoft.Xna.Framework.GameComponent.Game"/> property.
        /// </summary>
        protected new DiamondHollowGame Game { get => (DiamondHollowGame)base.Game; }
        /// <summary>
        /// The <see cref="DiamondHollow.Level"/> this component is attached to.
        /// Equivalent to <see cref="DiamondHollow.DiamondHollowGame.Level?name=Game.Level"/>.
        /// </summary>
        public Level Level;

        /// <summary>
        /// A record of all countdowns this component has created.
        /// </summary>
        private Dictionary<int, Countdown> _countdowns;
        /// <summary>
        /// A record holding information about a countdown.
        /// </summary>
        private struct Countdown
        {
            /// <summary>The duration of the countdown in ticks</summary>
            public int Duration;
            /// <summary>Whether the countdown should repeat after it is finished</summary>
            public bool Repeating;
            /// <summary>The callbacks to call when the countdown is finished</summary>
            public List<Action> Callbacks;
            /// <summary>The amount of ticks elapsed since the countdown was started</summary>
            public int Elapsed;
            /// <summary>Whether the countdown is finished</summary>
            public bool Done;

            /// <summary>
            /// Creates a new countdown with the given duration and whether it should repeat.
            /// </summary>
            /// <param name="duration">The duration of the countdown in ticks</param>
            /// <param name="repeating">Whether the countdown should repeat after it is finished</param>
            /// <param name="callbacks">The callbacks to call when the countdown is finished</param>
            /// <param name="elapsed">The amount of ticks elapsed since the countdown was started</param>
            /// <param name="done">Whether the countdown is finished</param>
            /// <returns>The new countdown</returns>
            public Countdown(int duration, bool repeating, List<Action> callbacks, int elapsed = 0, bool done = false)
            {
                Duration = duration;
                Repeating = repeating;
                Callbacks = callbacks;
                Elapsed = elapsed;
                Done = done;
            }

            /// <summary>
            /// Increment the countdown.
            /// </summary>
            public void Update()
            {
                if (!Repeating && Done) return;
                if (Elapsed >= Duration)
                {
                    Done = true;
                    Callbacks.ForEach(callback => callback.Invoke());
                    if (Repeating) Elapsed = 0;
                }
                else Elapsed++;
            }
            /// <summary>
            /// Reset the countdown.
            /// </summary>
            public void Reset()
            {
                Elapsed = 0;
                Done = false;
            }
            /// <summary>
            /// Check if the countdown is done.
            /// </summary>
            /// <returns>True if the countdown is done.</returns>
            public bool IsUp()
            {
                bool _done = Done;
                Done = false;
                return _done;
            }
        }

        /// <summary>
        /// Create a new Diamond Hollow game component.
        /// </summary>
        /// <param name="game">The game this component is attached to.</param>
        /// <param name="level">The level this component is attached to.</param>
        /// <returns>The new game component.</returns>
        public DHGameComponent(DiamondHollowGame game, Level level) : base(game)
        {
            Level = level;
            _countdowns = new Dictionary<int, Countdown>();
        }

        /// <summary>
        /// Create a new countdown.
        /// </summary>
        /// <param name="id">A new unique id for the countdown.</param>
        /// <param name="duration">The duration of the countdown, in ticks.</param>
        /// <param name="repeating">Whether the countdown should repeat automatically.</param>
        /// <param name="offset">The initial offset of the countdown, in ticks.</param>
        /// <param name="callback">The callback to call when the countdown is finished.</param>
        public void CreateCountdown(int id, int duration, bool repeating, int offset = 0, Action callback = null)
        {
            _countdowns[id] = new Countdown(duration, repeating, new List<Action>(), offset);
            if (callback != null) _countdowns[id].Callbacks.Add(callback);
        }

        /// <summary>
        /// Add a callback to an existing countdown.
        /// </summary>
        /// <param name="id">The id of the countdown.</param>
        /// <param name="callback">The callback to add.</param>
        public void AddCountdownCallback(int id, Action callback)
        {
            _countdowns[id].Callbacks.Add(callback);
        }

        /// <summary>
        /// Check if a countdown is done.
        /// </summary>
        /// <param name="id">The id of the countdown.</param>
        /// <returns>True if the countdown is done.</returns>
        /// <remarks>
        /// Only returns true once, subsequent calls return false.
        /// </remarks>
        protected bool IsCountdownDone(int id) => _countdowns[id].IsUp();

        /// <summary>
        /// Get the progress of a countdown.
        /// </summary>
        /// <param name="id">The id of the countdown.</param>
        /// <returns>The ratio of the countdown's progress, from 0 to 1.</returns>
        protected float GetCountdownProgress(int id) => (float)_countdowns[id].Elapsed / _countdowns[id].Duration;

        /// <summary>
        /// Restart a countdown.
        /// </summary>
        /// <param name="id">The id of the countdown.</param>
        protected void ResetCountdown(int id)
        {
            var cnd = _countdowns[id];
            cnd.Reset();
            _countdowns[id] = cnd;
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Updates all countdowns.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Records are immutable, so they have to be copied
            _countdowns = _countdowns.Select(kvp =>
            {
                var cnd = kvp.Value;
                cnd.Update();
                return (kvp.Key, Value: cnd);
            }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}