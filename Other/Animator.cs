using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    public class Animator : DHGameComponent
    {
        private Texture2D _texture;
        private readonly string _filename;
        private int _time;

        private Rectangle _cutout;
        private Point _offset;
        private int _frames;
        private readonly int _duration;

        public Animator(DiamondHollowGame game, Level level, string filename, int duration, Rectangle? cutout = null) : base(game, level)
        {
            _filename = filename;
            _duration = duration;
            _cutout = cutout ?? new(0, 0, 0, 0);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _texture = Game.Content.Load<Texture2D>(_filename);
            _time = 0;

            if (_texture.Width >= _texture.Height)
            {
                _frames = _texture.Width / _texture.Height;
                _offset = new(_texture.Height, 0);
            }
            else
            {
                _frames = _texture.Height / _texture.Width;
                _offset = new(0, _texture.Width);
            }

            if (_cutout.Width == 0) _cutout = new(0, 0, _offset.X + _offset.Y, _offset.X + _offset.Y);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _time++;
        }

        private Rectangle AnimationCutout => new(_offset.Scale(_time / _duration % _frames) + _cutout.Location, _cutout.Size);

        public void Draw(Rectangle bounds)
        {
            Game.SpriteBatch.Draw(_texture, bounds, AnimationCutout, Color.White);
        }

        public void DrawBatch(Rectangle bounds)
        {
            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            Draw(bounds);
            Game.SpriteBatch.End();
        }
    }
}