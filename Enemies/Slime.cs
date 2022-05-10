using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Slime : Enemy
    {
        public static new readonly Point Size = new(32);

        public Slime(DiamondHollowGame game, EnemyController controller, Point position)
            : base(game, controller, 20, new Rectangle(position - Size.Half(), Size))
        {
            Velocity = new Vector2(3, 0);
        }

        public override void Update(GameTime gameTime)
        {
            if (Level.IsOnGround(Bounds) && !Level.IsOnGround(Bounds.OffsetX((int)(Math.Sign(Velocity.X) * Size.X * 1.1f)))) Velocity.X = -Velocity.X;
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