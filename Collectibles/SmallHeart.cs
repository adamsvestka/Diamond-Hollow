using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // Picking this item up restore one heart for the player (cannot go above the player's max health)
    // Is not attracted to the player
    public class SmallHeart : Collectible
    {
        public static new readonly Point Size = new(30);

        public static Animator Animator;

        public SmallHeart(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 0f;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Animator.DrawBatch(Bounds.ToScreen());
        }
    }
}