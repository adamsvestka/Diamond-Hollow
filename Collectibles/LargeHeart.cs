using Microsoft.Xna.Framework;

namespace DiamondHollow
{
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