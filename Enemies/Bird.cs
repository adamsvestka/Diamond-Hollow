using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Bird : Enemy
    {
        public static new readonly Point Size = new(72, 48);

        public Bird(DiamondHollowGame game, EnemyController controller, Point position)
            : base(game, controller, 40, new Rectangle(position - Size.Half(), Size))
        {
            Velocity = new Vector2(3, 0);
            Gravity = 0;
        }

        public override void Update(GameTime gameTime)
        {
            var prev = Velocity;

            base.Update(gameTime);

            if (Velocity == Vector2.Zero) Velocity = -prev;
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