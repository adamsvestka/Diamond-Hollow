using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class CeilingShooter : Enemy
    {
        public static new readonly Point Size = new(32);
        public Vector2 Targeting;
        private int _shotCountdown;

        public CeilingShooter(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, new Rectangle(position - Size.Half(), Size))
        {
            Targeting = Vector2.Zero;
            _shotCountdown = 1;
            Gravity = 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Targeting = (Level.Player.Center - Center).ToVector2();
            Targeting.Normalize();

            if (--_shotCountdown == 0)
            {
                _shotCountdown = 250;
                if (Targeting.Y > 0) return;
                Level.ProjectileController.Spawn(new ProjectileConstructor
                {
                    Owner = this,
                    Origin = Center + Targeting.ToPoint(),
                    Direction = Targeting,
                    Size = new Point(20),
                    Speed = 5,
                    Color = Color.Red,
                });
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();
            Game.Level.DrawRectangle(Bounds, Color.Red);
            Game.SpriteBatch.End();
        }
    }
}