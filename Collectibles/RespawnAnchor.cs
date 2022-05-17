using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class RespawnAnchor : Collectible
    {
        public static new readonly Point Size = new(30);

        public RespawnAnchor(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 0f;
            OnCollect += self =>
            {
                Level.Spawnpoint = self.Center;
            };
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();
            Level.DrawRectangle(Bounds, Color.Goldenrod);
            Game.SpriteBatch.End();
        }
    }
}