using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class WallShooter : Enemy
    {
        public static new readonly Point Size = new(32);
        public Vector2 Targeting;
        private int _shotCountdown;

        public WallShooter(DiamondHollowGame game, EnemyController controller, Point position, bool facingRight)
            : base(game, controller, 20, new Rectangle(position - Size.Half(), Size))
        {
            Targeting = new Vector2(facingRight ? 1 : -1, 0);
            _shotCountdown = 1;
            Gravity = 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (--_shotCountdown == 0)
            {
                _shotCountdown = 250;
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
            Game.SpriteBatch.Begin();
            Game.Level.DrawRectangle(Bounds, Color.Red);
            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}