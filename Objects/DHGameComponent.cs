using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // The base class for all game components
    public class DHGameComponent : DrawableGameComponent
    {
        protected new DiamondHollowGame Game { get => (DiamondHollowGame)base.Game; }
        public Level Level;

        // Countdowns are timers used to execute a callback function after a certain amount of ticks
        // Countdowns are identifed by a numerical id, subclasses can define an enum of countdown names for this purpose
        // A coutdown is created with the CreateCountdown method and can be used in a few ways:
        //     - Countdowns can have a callback registered, which is called when the countdown is finished, additional callbacks can be added with AddCountdownCallback
        //     - The state of countdowns can be checked with IsCountdownDone and GetCountdownProgress methods
        //     - Countdowns can automatically repeat or be reset manually with the ResetCountdown method
        private Dictionary<int, Countdown> _countdowns;
        private record struct Countdown(int Duration, bool Repeating, List<Action> Callbacks, int Elapsed = 0, bool Done = false)
        {
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
            public void Reset()
            {
                Elapsed = 0;
                Done = false;
            }
            public bool IsUp()
            {
                bool _done = Done;
                Done = false;
                return _done;
            }
        }

        public DHGameComponent(DiamondHollowGame game, Level level) : base(game)
        {
            Level = level;
            _countdowns = new Dictionary<int, Countdown>();
        }

        protected void CreateCountdown(int id, int duration, bool repeating, int offset = 0, Action callback = null)
        {
            _countdowns[id] = new Countdown(duration, repeating, new List<Action>(), offset);
            if (callback != null) _countdowns[id].Callbacks.Add(callback);
        }

        public void AddCountdownCallback(int id, Action callback)
        {
            _countdowns[id].Callbacks.Add(callback);
        }

        protected bool IsCountdownDone(int id) => _countdowns[id].IsUp();   // Only returns true once, subsequent calls return false
        protected float GetCountdownProgress(int id) => (float)_countdowns[id].Elapsed / _countdowns[id].Duration;  // Returns a value between 0 and 1

        protected void ResetCountdown(int id)
        {
            var cnd = _countdowns[id];
            cnd.Reset();
            _countdowns[id] = cnd;
        }

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