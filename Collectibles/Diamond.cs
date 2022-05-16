using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Diamond : Collectible
    {
        public static new readonly Point Size = new(20);

        public Diamond(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 4f;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();
            Game.Level.DrawRectangle(Bounds, Color.Green);
            Game.SpriteBatch.End();
        }
    }
}