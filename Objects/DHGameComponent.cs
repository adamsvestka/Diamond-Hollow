using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class DHGameComponent : DrawableGameComponent
    {
        protected new DiamondHollowGame Game { get => (DiamondHollowGame)base.Game; }
        public Level Level;

        public DHGameComponent(DiamondHollowGame game, Level level) : base(game)
        {
            Level = level;
        }
    }
}