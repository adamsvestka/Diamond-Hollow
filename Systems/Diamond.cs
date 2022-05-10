using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Diamond : CollisionBody
    {
        public DiamondController Controller;
        public static new readonly Point Size = new(20);

        public event Action OnCollect;

        public Diamond(DiamondHollowGame game, DiamondController controller, Point pos) : base(game, controller.Level, new Rectangle(pos - Size.Half(), Size))
        {
            Controller = controller;
            DisableCollisionBox = true;
        }

        public override void Update(GameTime gameTime)
        {
            var distance = (Level.Player.Center - Center).ToVector2().Length() / Game.TileSize;
            if (distance < 1)
            {
                Controller.Despawn(this);
                OnCollect?.Invoke();
            }
            else if (distance < 3)
            {
                var strength = (3 - distance) / 10;
                Velocity = (Level.Player.Center.ToVector2() - Center.ToVector2()) * strength;
            }
            else Velocity *= 0.9f;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();
            Game.Level.DrawRectangle(Bounds, Color.Green);
            Game.SpriteBatch.End();
        }
    }
}