using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class LargeHeart : Collectible
    {
        public static new readonly Point Size = new(40);

        public LargeHeart(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 0f;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();
            Game.Level.DrawRectangle(Bounds, Color.MediumVioletRed);
            Game.SpriteBatch.End();
        }
    }
}