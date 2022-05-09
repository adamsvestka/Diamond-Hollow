using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class GameComponent : DrawableGameComponent
    {
        protected new DiamondHollowGame Game { get => (DiamondHollowGame)base.Game; }

        public GameComponent(DiamondHollowGame game) : base(game) { }
    }
}