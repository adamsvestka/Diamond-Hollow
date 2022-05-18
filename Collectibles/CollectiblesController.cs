using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class CollectiblesController : DHGameComponent
    {
        private readonly List<Collectible> _collectibles;

        public CollectiblesController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _collectibles = new List<Collectible>();

            Level.AddComponents(
                Diamond.Animator = new(Game, Level, "Sprites/Diamond", 20, new(4, 4, 8, 8)),
                SmallHeart.Animator = new(Game, Level, "Sprites/Heart1", 20, new(3, 3, 10, 10)),
                LargeHeart.Animator = new(Game, Level, "Sprites/Heart2", 20, new(1, 2, 14, 14)),
                RespawnAnchor.Animator = new(Game, Level, "Sprites/Star", 20, new(4, 4, 24, 24))
            );
        }

        public void SpawnDiamond(Point position)
        {
            var diamond = new Diamond(Game, this, position);
            diamond.OnCollect += Level.Player.OnItemCollision;
            _collectibles.Add(diamond);
            Level.AddComponent(diamond);
        }

        public void SpawnDiamondCluster(Point position, int count, float dispersion = 5)
        {
            for (int i = 0; i < count; i++)
            {
                var diamond = new Diamond(Game, this, position)
                {
                    Velocity = Vector2.Zero.RandomOffset(dispersion)
                };
                diamond.OnCollect += Level.Player.OnItemCollision;
                _collectibles.Add(diamond);
                Level.AddComponent(diamond);
            }
        }

        public void SpawnSmallHeart(Point position)
        {
            var heart = new SmallHeart(Game, this, position);
            heart.OnCollect += Level.Player.OnItemCollision;
            _collectibles.Add(heart);
            Level.AddComponent(heart);
        }

        public void SpawnLargeHeart(Point position)
        {
            var heart = new LargeHeart(Game, this, position);
            heart.OnCollect += Level.Player.OnItemCollision;
            _collectibles.Add(heart);
            Level.AddComponent(heart);
        }

        public void SpawnNuke(Point position)
        {
            var nuke = new Nuke(Game, this, position);
            _collectibles.Add(nuke);
            Level.AddComponent(nuke);
        }

        public void SpawnRespawnAnchor(Point position)
        {
            var anchor = new RespawnAnchor(Game, this, position);
            _collectibles.Add(anchor);
            Level.AddComponent(anchor);
        }

        internal void Despawn(Collectible collectible)
        {
            _collectibles.Remove(collectible);
            Level.RemoveComponent(collectible);
        }
    }
}