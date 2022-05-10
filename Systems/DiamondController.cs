using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class DiamondController : DHGameComponent
    {
        private readonly List<Diamond> _diamonds;

        public DiamondController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _diamonds = new List<Diamond>();
        }

        public void Spawn(Point position)
        {
            var diamond = new Diamond(Game, this, position);
            _diamonds.Add(diamond);
            Level.AddComponent(diamond);
        }

        public void SpawnCluster(Point position, int count, float dispersion = 5)
        {
            for (int i = 0; i < count; i++)
            {
                var diamond = new Diamond(Game, this, position)
                {
                    Velocity = Vector2.Zero.RandomOffset(dispersion),
                    
                };
                _diamonds.Add(diamond);
                Level.AddComponent(diamond);
            }
        }

        internal void Despawn(Diamond diamond)
        {
            _diamonds.Remove(diamond);
            Level.RemoveComponent(diamond);
        }
    }
}