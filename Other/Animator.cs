using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    // For managing multiple animations for a single object or the same animation across multiple objects
    public class Animator : DHGameComponent
    {
        // Texture: A base texture
        // Cutout: The position and size of the first frame of the animation
        // Offset: The delta position between frames of the animation
        // Frames: The number of frames in the animation
        public record struct Animation(Texture2D[] Textures, Rectangle Cutout, Point Offset, int Frames)
        {
            public Animation(Texture2D[] textures, Rectangle? cutout) : this(textures, cutout ?? Rectangle.Empty, Point.Zero, 0)
            {
                // Automatically determines the delta offset and number of frames
                var Texture = Textures[0];
                if (Texture.Width >= Texture.Height)
                {
                    Frames = Texture.Width / Texture.Height;
                    Offset = new(Texture.Height, 0);
                }
                else
                {
                    Frames = Texture.Height / Texture.Width;
                    Offset = new(0, Texture.Width);
                }
                if (Cutout == Rectangle.Empty) Cutout = new(0, 0, Offset.X + Offset.Y, Offset.X + Offset.Y);
            }
            public Rectangle GetFrame(int frame) => new(Offset.Scale(frame) + Cutout.Location, Cutout.Size);    // Get cutout for the current frame
        }

        private readonly List<(string, string[], Rectangle)> _files;
        private int _time;              // Elapsed time from beginning of animation
        private readonly int _duration; // Duration of each frame in ticks
        private bool _paused;

        private readonly Dictionary<string, Animation> _states;     // Different animations for different states
        private string _current;    // Current animation state
        private Action _complete;   // Callback to run when animation completes
        public Animation Anim => _states[_current];
        public Rectangle Frame => Anim.GetFrame(_time / _duration);

        public string State => _current;

        // An animation object can have multiple states, each with unique animation
        // States are keyed by strings, the default key is "default
        // Animating can be paused/resumed and a callback can be set to run when an animation loop completes
        public Animator(DiamondHollowGame game, Level level, string filename, int duration, Rectangle? cutout = null) : base(game, level)
        {
            _files = new() { ("default", new[] { filename }, cutout ?? Rectangle.Empty) };
            _duration = duration;
            _states = new();
            _paused = false;
        }

        public void AddState(string name, Rectangle? cutout, params string[] filenames)
        {
            _files.Add((name, filenames, cutout ?? Rectangle.Empty));
        }

        public bool HasState(string name) => _states.ContainsKey(name);

        protected override void LoadContent()
        {
            base.LoadContent();

            _time = 0;

            foreach (var (state, filenames, cutout) in _files)
            {
                _states.Add(state, new(filenames.Select(Game.GetTexture).ToArray(), cutout));
            }
            _current = "default";
        }

        public bool IsPlaying() => !_paused;
        public void Pause() => _paused = true;
        public void Resume() => _paused = false;

        public void PlayState(string state, Action callback = null)
        {
            if (_complete != null) return;
            _paused = false;
            _current = state;
            _complete = callback;
            _time %= _duration;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!_paused && ++_time >= _duration * Anim.Frames)
            {
                _time = 0;
                _complete?.Invoke();
                _complete = null;
                _current = "default";
            }
        }

        public void Draw(Rectangle bounds, bool flipped = false, float alpha = 1f)
        {
            foreach (var texture in Anim.Textures)
            {
                Game.SpriteBatch.Draw(texture, bounds, Frame, Color.White * alpha, 0, Vector2.Zero, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
        }

        public void DrawBatch(Rectangle bounds, bool flipped = false, float alpha = 1f)
        {
            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            Draw(bounds, flipped, alpha);
            Game.SpriteBatch.End();
        }
    }
}