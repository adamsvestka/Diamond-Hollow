using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // Component that have a callback when collected by the player
    public class Collectible : CollisionBody
    {
        public CollectiblesController Controller;
        public float AttractionStrength = 0f;   // The item will move towards the player if within this distance, distance is measured in tiles
        public float? PickupDistance = null;    // The item will be picked up if within this distance, without having to actually collide with the player

        public event Action<Collectible> OnCollect;

        public Collectible(DiamondHollowGame game, CollectiblesController controller, Rectangle bounds) : base(game, controller.Level, bounds)
        {
            Controller = controller;
            DrawOrder = (int)DrawingLayers.Collectibles;
            DisableCollisionBox = true;
        }

        public override void Update(GameTime gameTime)
        {
            float distance = (Level.Player.Center - Center).ToVector2().Length() / Game.TileSize;
            if (distance < (PickupDistance ?? (Level.Player.Size.X + Size.X) * 0.7f / Game.TileSize))   // Pickup distance
            {
                OnCollect?.Invoke(this);
                Controller.Despawn(this);
            }
            else if (distance < AttractionStrength)     // Attraction distance
            {
                // Some kind of non-linear scaling with closeness to the player
                float strength = (float)Math.Pow(AttractionStrength - distance, 2);
                Vector2 direction = Level.Player.Center.ToVector2() - Center.ToVector2();
                direction.Normalize();
                Velocity = Velocity * 0.7f + direction * strength * 0.3f;
            }
            else Velocity *= 0.9f;  // Otherwise, lose momentum

            base.Update(gameTime);
        }
    }
}