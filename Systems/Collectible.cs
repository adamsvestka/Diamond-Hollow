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
            var distance = (Level.Player.Center - Center).ToVector2().Length() / Game.TileSize;
            if (distance < 1)
            {
                OnCollect?.Invoke(this);
                Controller.Despawn(this);
            }
            else if (distance < AttractionStrength)
            {
                var strength = (AttractionStrength - distance) / 10;
                Velocity = (Level.Player.Center.ToVector2() - Center.ToVector2()) * strength;
            }
            else Velocity *= 0.9f;

            base.Update(gameTime);
        }
    }
}