using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// Picking this item up restores one heart for the player (cannot go above the player's max health).
    /// It is not attracted to the player.
    /// </summary>
    public class SmallHeart : Collectible
    {
        /// <summary>
        /// The size of a small heart.
        /// </summary>
        public static new readonly Point Size = new(30);

        /// <summary>
        /// The small heart's animation handler.
        /// </summary>
        public static Animator Animator;

        /// <summary>
        /// Creates a new small heart at the given position.
        /// 
        /// Do not use this constructor directly. Instead, use the <see cref="DiamondHollow.CollectiblesController.SpawnSmallHeart"/> method.
        /// </summary>
        /// <param name="game">The game that this component is a part of.</param>
        /// <param name="controller">The level that this component is a part of.</param>
        /// <param name="pos">The position of the small heart.</param>
        /// <returns>The new small heart.</returns>
        public SmallHeart(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 0f;
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draws the animating small heart.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Animator.DrawBatch(Bounds.ToScreen());
        }
    }
}