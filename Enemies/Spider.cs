using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    enum SpiderState
    {
        Armed,
        Falling,
        Paused,
        Rewinding,
    }

    public class Spider : Enemy
    {
        public static new readonly Point Size = new(32);
        private SpiderState _state;
        private int _cooldown;

        public Point Origin;
        public bool IsOnCeiling => Center == Origin;

        public Spider(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, new Rectangle(position - Size.Half(), Size))
        {
            Origin = Center;
            _state = SpiderState.Armed;
            _cooldown = 0;
            Gravity = 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_cooldown > 0)
            {
                _cooldown--;
                return;
            }

            switch (_state)
            {
                case SpiderState.Armed when CheckForPlayer():
                    _state = SpiderState.Falling;
                    Gravity = 1f;
                    break;
                case SpiderState.Falling when IsOnGround:
                    _state = SpiderState.Paused;
                    _cooldown = 60;
                    break;
                case SpiderState.Paused:
                    _state = SpiderState.Rewinding;
                    Velocity = new Vector2(0, 3);
                    Gravity = 0;
                    break;
                case SpiderState.Rewinding when IsOnCeiling:
                    _state = SpiderState.Armed;
                    Velocity = Vector2.Zero;
                    _cooldown = 60;
                    break;
            }
        }

        private bool CheckForPlayer()
        {
            var box = Bounds;
            for (int i = 0; i < 8 * Game.TileSize / Size.Y; i++)
            {
                box.Y -= Size.Y;
                if (box.Intersects(Level.Player.Bounds)) return true;
            }
            return false;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();
            Game.Level.DrawRectangle(Bounds, Color.Red);
            Game.Level.DrawLine(Origin.OffsetY(Size.Y / 2), Center.OffsetY(Size.Y / 2), Color.Red, 4);
            Game.SpriteBatch.End();
        }
    }
}