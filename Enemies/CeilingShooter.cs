using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class CeilingShooter : Enemy
    {
        private enum Countdowns { Shoot }

        public static new readonly Point Size = new(32);
        public Vector2 Targeting;

        public CeilingShooter(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, 30, new Rectangle(position - Size.Half(), Size))
        {
            Targeting = Vector2.Zero;
            Gravity = 0;

            CreateCountdown((int)Countdowns.Shoot, (int)(250 / Level.Modifier), true, 0, () =>
            {
                if (Targeting.Y > 0) return;
                Level.ProjectileController.Spawn(new ProjectileConstructor
                {
                    Owner = this,
                    Origin = Center,
                    Direction = Targeting,
                    Size = new Point(20),
                    Speed = 5 * Level.Modifier,
                    Color = Color.Red,
                });
            });
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Targeting = (Level.Player.Center - Center).ToVector2();
            Targeting.Normalize();
        }

        public override void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Begin();
            Level.DrawRectangle(Bounds, Color.Red);
            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}