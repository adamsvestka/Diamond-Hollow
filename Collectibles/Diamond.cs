using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Diamond : Collectible
    {
        public static new readonly Point Size = new(24);

        public static Animator Animator;

        public Diamond(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 4f;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Animator.DrawBatch(Bounds.ToScreen());
        }
    }
}