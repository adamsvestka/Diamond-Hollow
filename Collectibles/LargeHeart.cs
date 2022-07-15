using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // Picking this item up restores all health for the player and increases their max health by one
    // Is not attracted to the player, but has a large hitbox
    public class LargeHeart : Collectible
    {
        public static new readonly Point Size = new(42);

        public static Animator Animator;

        public LargeHeart(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
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