using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class SmallHeart : Collectible
    {
        public static new readonly Point Size = new(20);

        public SmallHeart(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 0f;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();
            Game.Level.DrawRectangle(Bounds, Color.PaleVioletRed);
            Game.SpriteBatch.End();
        }
    }
}