using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    /// <summary>
    /// This item is different from the others as it's not really an item.
    /// "Picking it up" will move the players spawn point to this location.
    /// It's not attracted to the player, but has a decent pickup distance.
    ///
    /// This item's design is really subtle - it blends in with the level's background.
    /// Stars will spawn around it, though, so be on the look out for those.
    /// </summary>
    public class RespawnAnchor : Collectible
    {
        /// <summary>
        /// The size of a respawn anchor.
        /// </summary>
        public static new readonly Point Size = new(30);

        /// <summary>
        /// The respawn anchor's animation handler.
        /// </summary>
        public static Animator Animator;

        /// <summary>
        /// A list of stars that are spawned around the respawn anchor.
        /// </summary>
        private List<Rectangle> _stars;

        /// <summary>
        /// Creates a new respawn anchor at the given position.
        /// 
        /// Do not use this constructor directly. Instead, use the <see cref="DiamondHollow.CollectiblesController.SpawnRespawnAnchor"/> method.
        /// </summary>
        /// <param name="game">The game that this component is a part of.</param>
        /// <param name="controller">The level that this component is a part of.</param>
        /// <param name="pos">The position of the respawn anchor.</param>
        /// <returns>The new respawn anchor.</returns>
        public RespawnAnchor(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 0f;
            PickupDistance = 1.5f;
            DrawOrder = (int)DrawingLayers.Background + 1;
            OnCollect += self => Level.Spawnpoint = self.Center;
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Initialize"/>
        /// <summary>
        /// Randomly spawns stars around the respawn anchor.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Choose positions for the stars to spawn around the anchor, make sure they don't collide with walls
            // Happens in the Initialize method because it needs to be done after the level is loaded
            _stars = new();
            var starSize = new Point(24, 24);
            for (int i = 0; i < 7 && _stars.Count < 3; i++)
            {
                var star = new Rectangle(Center.RandomOffset(4 * Game.TileSize) - starSize.Half(), starSize);
                if (star.Corners().Any(p => Level.IsWall(p)) || _stars.Any(s => (s.Center - star.Center).Length() < 50)) continue;
                _stars.Add(star);
            }
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draws the animating respawn anchor.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            var rect = Position.SnapToGrid().MakeTile().ToScreen();
            // The anchor is made up of two textures
            Game.SpriteBatch.Draw(Level._backgroundTileset, rect, new Rectangle(Level._backgroundTileMap.Keys.ElementAt(6).Scale(16), new Point(16)), Color.White * 0.9f);
            Game.SpriteBatch.Draw(Level._backgroundTileset, rect.OffsetY(-Game.TileSize), new Rectangle(Level._backgroundTileMap.Keys.ElementAt(5).Scale(16), new Point(16)), Color.White * 0.9f);

            _stars?.ForEach(star => Animator.Draw(star.ToScreen()));

            Game.SpriteBatch.End();
        }
    }
}