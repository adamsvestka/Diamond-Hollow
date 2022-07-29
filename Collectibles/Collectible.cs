using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// Component that have a callback when collected by the player.
    /// </summary>
    public class Collectible : CollisionBody
    {
        /// <summary>
        /// The controller for the collectible.
        /// </summary>
        public CollectiblesController Controller;
        /// <summary>
        /// The item will move towards the player if within this distance, distance is measured in tiles.
        /// </summary>
        public float AttractionStrength = 0f;
        /// <summary>
        /// The item will be picked up if within this distance, without having to actually collide with the player.
        /// </summary>
        public float? PickupDistance = null;

        /// <summary>
        /// The callback that will be called when the collectible is collected by the player.
        /// </summary>
        public event Action<Collectible> OnCollect;

        /// <summary>
        /// Creates a new collectible.
        /// </summary>
        /// <param name="game">The game that the collectible belongs to.</param>
        /// <param name="controller">The controller for the collectible.</param>
        /// <param name="bounds">The bounds of the collectible.</param>
        /// <returns>The new collectible.</returns>
        public Collectible(DiamondHollowGame game, CollectiblesController controller, Rectangle bounds) : base(game, controller.Level, bounds)
        {
            Controller = controller;
            DrawOrder = (int)DrawingLayers.Collectibles;
            DisableCollisionBox = true;
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Checks if the collectible is within collectible range of the player and if so, calls the OnCollect event and removes the collectible.
        /// - Collectibles are attracted to the player if they are within attraction strength. The closer they are, the faster they will move towards the player.
        /// - If they are outside the attraction strength, they will slow down.
        /// </summary>
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