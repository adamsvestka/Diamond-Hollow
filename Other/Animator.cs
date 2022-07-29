using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    /// <summary>
    /// Used for managing multiple animations for a single object or the same animation across multiple objects.
    /// </summary>
    public class Animator : DHGameComponent
    {
        /// <summary>
        /// A struct holding information about an animation.
        /// </summary>
        public struct Animation
        {
            /// <summary>
            /// The textures of the animation.
            /// </summary>
            public Texture2D[] Textures;
            /// <summary>
            /// The position and size of the first frame of the animation.
            /// </summary>
            public Rectangle Cutout;
            /// <summary>
            /// The delta position between frames of the animation.
            /// </summary>
            public Point Offset;
            /// <summary>
            /// The number of frames in the animation.
            /// </summary>
            public int Frames;

            /// <summary>
            /// Creates a new animation.
            /// </summary>
            /// <param name="textures">The textures of the animation.</param>
            /// <param name="cutout">The position and size of the first frame of the animation.</param>
            /// <param name="offset">The delta position between frames of the animation.</param>
            /// <param name="frames">The number of frames in the animation.</param>
            public Animation(Texture2D[] textures, Rectangle cutout, Point offset, int frames)
            {
                Textures = textures;
                Cutout = cutout;
                Offset = offset;
                Frames = frames;
            }

            /// <summary>
            /// Creates a new animation.
            /// </summary>
            /// <param name="textures">The textures of the animation.</param>
            /// <param name="cutout">The position and size of the first frame of the animation.</param>
            /// <returns>The new animation.</returns>
            /// <remarks>
            /// Automatically determines the delta offset and number of frames.
            /// </remarks>
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
            /// <summary>
            /// Get cutout for the current frame.
            /// </summary>
            /// <param name="frame">The index of the current frame.</param>
            /// <returns>The cutout for the current frame.</returns>
            public Rectangle GetFrame(int frame) => new(Offset.Scale(frame) + Cutout.Location, Cutout.Size);
        }

        /// <summary>
        /// Texture files of the animations.
        /// </summary>
        private readonly List<(string, string[], Rectangle)> _files;
        /// <summary>
        /// Elapsed time from beginning of animation.
        /// </summary>
        private int _time;
        /// <summary>
        /// Duration of each frame in ticks.
        /// </summary>
        private readonly int _duration;
        /// <summary>
        /// Whether the animation is paused.
        /// </summary>
        private bool _paused;

        /// <summary>
        /// Different animations for different states.
        /// </summary>
        private readonly Dictionary<string, Animation> _states;
        /// <summary>
        /// Current animation state.
        /// </summary>
        private string _current;
        /// <summary>
        /// Callback to run when animation completes.
        /// </summary>
        private Action _complete;
        /// <summary>
        /// An alias to the current animation state.
        /// </summary>
        public Animation Anim => _states[_current];
        /// <summary>
        /// The current cutout frame of the animation.
        /// </summary>
        public Rectangle Frame => Anim.GetFrame(_time / _duration);

        /// <summary>
        /// Alias for <see cref="DiamondHollow.Animator._current"/>.
        /// </summary>
        public string State => _current;

        /// <summary>
        /// An animation object can have multiple states, each with unique animation.
        /// States are keyed by strings, the default key is <c>"default"</c>.
        /// Animating can be paused/resumed and a callback can be set to run when an animation loop completes.
        /// </summary>
        /// <param name="game">The game this component is attached to.</param>
        /// <param name="level">The level this component is attached to.</param>
        /// <param name="filename">The filename of the animation file.</param>
        /// <param name="duration">The duration of each frame in ticks.</param>
        /// <param name="cutout">The position and size of the first frame of the animation.</param>
        /// <returns>The new animator.</returns>
        public Animator(DiamondHollowGame game, Level level, string filename, int duration, Rectangle? cutout = null) : base(game, level)
        {
            _files = new() { ("default", new[] { filename }, cutout ?? Rectangle.Empty) };
            _duration = duration;
            _states = new();
            _paused = false;
        }

        /// <summary>
        /// An animation object can have multiple states, each with unique animation. Add a new state to the animation.
        /// </summary>
        /// <param name="name">The name of the state.</param>
        /// <param name="cutout">The position and size of the first frame of the animation.</param>
        /// <param name="filenames">The filenames of the animation.</param>
        public void AddState(string name, Rectangle? cutout, params string[] filenames)
        {
            _files.Add((name, filenames, cutout ?? Rectangle.Empty));
        }

        /// <summary>
        /// Check if the animation has a state registered under a name.
        /// </summary>
        /// <param name="name">The name of the state.</param>
        /// <returns>Whether the animation has a state registered under the name.</returns>
        public bool HasState(string name) => _states.ContainsKey(name);

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.LoadContent"/>
        /// <summary>
        /// Loads the textures of the animation.
        /// </summary>
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

        /// <summary>
        /// Check if an animation is currently playing.
        /// </summary>
        /// <returns>True if an animation is playing.</returns>
        public bool IsPlaying() => !_paused;
        /// <summary>
        /// Pause the animation.
        /// </summary>
        public void Pause() => _paused = true;
        /// <summary>
        /// Resume the animation.
        /// </summary>
        public void Resume() => _paused = false;

        /// <summary>
        /// Set the current animation state and possibly register a callback to run when the animation completes.
        /// </summary>
        /// <param name="state">The name of the state.</param>
        /// <param name="callback">The callback to run when the animation completes.</param>
        public void PlayState(string state, Action callback = null)
        {
            if (_complete != null) return;
            _paused = false;
            _current = state;
            _complete = callback;
            _time %= _duration;
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Update the animation frame.
        /// </summary>
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

        /// <summary>
        /// Draw the animation into a given rectangle.
        /// </summary>
        /// <param name="bounds">The rectangle to draw the animation into.</param>
        /// <param name="flipped">Whether the animation should be flipped horizontally.</param>
        /// <param name="alpha">The alpha of the animation.</param>
        public void Draw(Rectangle bounds, bool flipped = false, float alpha = 1f)
        {
            foreach (var texture in Anim.Textures)
            {
                Game.SpriteBatch.Draw(texture, bounds, Frame, Color.White * alpha, 0, Vector2.Zero, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
        }

        /// <summary>
        /// A convenience method for <see cref="DiamondHollow.Animator.Draw"/>.
        /// Wraps the call in a <see cref="Microsoft.Xna.Framework.Graphics.SpriteBatch"/> begin/end block.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="flipped"></param>
        /// <param name="alpha"></param>
        public void DrawBatch(Rectangle bounds, bool flipped = false, float alpha = 1f)
        {
            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            Draw(bounds, flipped, alpha);
            Game.SpriteBatch.End();
        }
    }
}