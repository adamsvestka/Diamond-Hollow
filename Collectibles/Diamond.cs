using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// Picking this item up increases the player's score.
    /// It is highly attracted to the player.
    /// </summary>
    public class Diamond : Collectible
    {
        /// <summary>
        /// The size of a diamond.
        /// </summary>
        /// <returns></returns>
        public static new readonly Point Size = new(24);

        /// <summary>
        /// The diamond's animation handler.
        /// </summary>
        public static Animator Animator;

        /// <summary>
        /// Creates a new diamond at the given position.
        /// 
        /// Do not use this constructor directly. Instead, use the <see cref="DiamondHollow.CollectiblesController.SpawnDiamond"/> method.
        /// </summary>
        /// <param name="game">The game that this component is a part of.</param>
        /// <param name="controller">The level that this component is a part of.</param>
        /// <param name="pos">The position of the diamond.</param>
        /// <returns>The new diamond.</returns>
        public Diamond(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 4f;
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draws the animating diamond.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Animator.DrawBatch(Bounds.ToScreen());
        }
    }
}