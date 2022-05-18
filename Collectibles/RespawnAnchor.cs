using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    public class RespawnAnchor : Collectible
    {
        public static new readonly Point Size = new(30);

        public static Animator Animator;

        private List<Rectangle> _stars;

        public RespawnAnchor(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 0f;
            PickupDistance = 1.5f;
            OnCollect += self => Level.Spawnpoint = self.Center;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_stars == null)
            {
                _stars = new();
                var starSize = new Point(24, 24);
                for (int i = 0; i < 7 && _stars.Count < 3; i++)
                {
                    var star = new Rectangle(Center.RandomOffset(4 * Game.TileSize) - starSize.Half(), starSize);
                    if (star.Corners().Any(p => Level.IsWall(p)) || _stars.Any(s => (s.Center - star.Center).Length() < 50)) continue;
                    _stars.Add(star);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            var rect = Position.SnapToGrid().MakeTile().ToScreen();
            Game.SpriteBatch.Draw(Level._backgroundTileset, rect, new Rectangle(Level._backgroundTileMap.Keys.ElementAt(6).Scale(16), new Point(16)), Color.White * 0.9f);
            Game.SpriteBatch.Draw(Level._backgroundTileset, rect.OffsetY(-Game.TileSize), new Rectangle(Level._backgroundTileMap.Keys.ElementAt(5).Scale(16), new Point(16)), Color.White * 0.9f);

            _stars?.ForEach(star =>
            {
                Animator.Draw(star.ToScreen());
                foreach (var corner in star.Corners())
                    Game.SpriteBatch.Draw(Game.WhitePixel, corner.ToScreen().ToVector2(), Color.White * 0.5f);
            });

            Game.SpriteBatch.End();
        }
    }
}