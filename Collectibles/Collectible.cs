using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Collectible : CollisionBody
    {
        public CollectiblesController Controller;
        public float AttractionStrength = 0f;

        public event Action<Collectible> OnCollect;

        public Collectible(DiamondHollowGame game, CollectiblesController controller, Rectangle bounds) : base(game, controller.Level, bounds)
        {
            Controller = controller;
            DisableCollisionBox = true;
        }

        public override void Update(GameTime gameTime)
        {
            float distance = (Level.Player.Center - Center).ToVector2().Length() / Game.TileSize;
            if (distance < (Level.Player.Size.X + Size.X) * 0.7f / Game.TileSize)
            {
                OnCollect?.Invoke(this);
                Controller.Despawn(this);
            }
            else if (distance < AttractionStrength)
            {
                float strength = (float)Math.Pow(AttractionStrength - distance, 2);
                Vector2 direction = Level.Player.Center.ToVector2() - Center.ToVector2();
                direction.Normalize();
                Velocity = Velocity * 0.7f + direction * strength * 0.3f;
            }
            else Velocity *= 0.9f;

            base.Update(gameTime);
        }
    }
}