using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    public class Animator : DHGameComponent
    {
        public record struct Animation(Texture2D[] Textures, Rectangle Cutout, Point Offset, int Frames)
        {
            public Animation(Texture2D[] textures, Rectangle? cutout) : this(textures, cutout ?? Rectangle.Empty, Point.Zero, 0)
            {
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
            public Rectangle GetFrame(int frame) => new(Offset.Scale(frame) + Cutout.Location, Cutout.Size);
        }

        private readonly List<(string, string[], Rectangle)> _files;
        private int _time;
        private readonly int _duration;

        private readonly Dictionary<string, Animation> _states;
        private string _current;
        private Action _complete;
        public Animation Anim => _states[_current];
        public Rectangle Frame => Anim.GetFrame(_time / _duration);

        public Animator(DiamondHollowGame game, Level level, string filename, int duration, Rectangle? cutout = null) : base(game, level)
        {
            _files = new() { ("default", new[] { filename }, cutout ?? Rectangle.Empty) };
            _duration = duration;
            _states = new();
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
                _states.Add(state, new(filenames.Select(Game.Content.Load<Texture2D>).ToArray(), cutout));
            }
            _current = "default";
        }

        public void PlayState(string state, Action callback = null)
        {
            _current = state;
            _complete = callback;
            _time %= _duration;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (++_time >= _duration * Anim.Frames)
            {
                _time = 0;
                _complete?.Invoke();
                _current = "default";
            }
        }

        public void Draw(Rectangle bounds, bool flipped = false)
        {
            foreach (var texture in Anim.Textures)
            {
                Game.SpriteBatch.Draw(texture, bounds, Frame, Color.White, 0, Vector2.Zero, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
        }

        public void DrawBatch(Rectangle bounds, bool flipped = false)
        {
            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            Draw(bounds, flipped);
            Game.SpriteBatch.End();
        }
    }
}