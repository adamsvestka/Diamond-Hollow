using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// Picking this item up restores all health for the player and increases their max health by one.
    /// It is not attracted to the player, but has a large hitbox.
    /// </summary>
    public class LargeHeart : Collectible
    {
        /// <summary>
        /// The size of a large heart.
        /// </summary>
        public static new readonly Point Size = new(42);

        /// <summary>
        /// The large heart's animation handler.
        /// </summary>
        public static Animator Animator;

        /// <summary>
        /// Creates a new large heart at the given position.
        /// 
        /// Do not use this constructor directly. Instead, use the <see cref="DiamondHollow.CollectiblesController.SpawnLargeHeart"/> method.
        /// </summary>
        /// <param name="game">The game that this component is a part of.</param>
        /// <param name="controller">The level that this component is a part of.</param>
        /// <param name="pos">The position of the large heart.</param>
        /// <returns>The new large heart.</returns>
        public LargeHeart(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 0f;
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draws the animating large heart.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Animator.DrawBatch(Bounds.ToScreen());
        }
    }
}