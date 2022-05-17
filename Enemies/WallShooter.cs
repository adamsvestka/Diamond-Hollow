using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class WallShooter : Enemy
    {
        private enum Countdowns { Shoot }

        public static new readonly Point Size = new(32);
        public Vector2 Targeting;

        public WallShooter(DiamondHollowGame game, EnemyController controller, Point position, bool facingRight) : base(game, controller, 20, new Rectangle(position - Size.Half(), Size))
        {
            Targeting = new Vector2(facingRight ? 1 : -1, 0);
            Gravity = 0;

            CreateCountdown((int)Countdowns.Shoot, (int)(250 / Level.Modifier), true, 100, () =>
            {
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

        public override void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Begin();
            Level.DrawRectangle(Bounds, Color.Red);
            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}