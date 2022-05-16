using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Spider : Enemy
    {
        private enum SpiderState { Armed, Falling, Paused, Rewinding }
        private enum Countdowns { Pause }

        public static new readonly Point Size = new(32);
        private SpiderState _state;

        public Point Origin;
        public bool IsOnCeiling => Center == Origin;

        public Spider(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, 20, new Rectangle(position - Size.Half(), Size))
        {
            Origin = Center;
            _state = SpiderState.Armed;
            Gravity = 0;

            CreateCountdown((int)Countdowns.Pause, (int)(60 / Level.Modifier), false);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            switch (_state)
            {
                case SpiderState.Armed when CheckForPlayer() && IsCountdownDone((int)Countdowns.Pause):
                    _state = SpiderState.Falling;
                    Gravity = 1f;
                    break;
                case SpiderState.Falling when IsOnGround:
                    _state = SpiderState.Paused;
                    ResetCountdown((int)Countdowns.Pause);
                    break;
                case SpiderState.Paused when IsCountdownDone((int)Countdowns.Pause):
                    _state = SpiderState.Rewinding;
                    Velocity = new Vector2(0, 3);
                    Gravity = 0;
                    break;
                case SpiderState.Rewinding when IsOnCeiling:
                    _state = SpiderState.Armed;
                    Velocity = Vector2.Zero;
                    ResetCountdown((int)Countdowns.Pause);
                    break;
            }
        }

        private bool CheckForPlayer()
        {
            int xdiff = Center.X - Level.Player.Center.X;
            int ydiff = Center.Y - Level.Player.Center.Y;
            return Math.Abs(xdiff) < Size.X / 2 + Level.Player.Size.X / 2 && 0 < ydiff && ydiff < 8 * Game.TileSize;
        }

        public override void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Begin();
            Game.Level.DrawRectangle(Bounds, Color.Red);
            Game.Level.DrawLine(Origin.OffsetY(Size.Y / 2), Center.OffsetY(Size.Y / 2), Color.Red, 4);
            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}