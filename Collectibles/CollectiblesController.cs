using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// Handles spawning and despawning of collectibles.
    /// Registers item pickups with the player.
    /// And has a utility method for spawning a randomly dispersing cluster of diamonds.
    /// </summary>
    public class CollectiblesController : DHGameComponent
    {
        /// <summary>
        /// A list of all the collectibles in the level.
        /// </summary>
        private readonly List<Collectible> _collectibles;

        /// <summary>
        /// Creates a new collectibles controller and registers collectible animators with the level.
        /// </summary>
        /// <param name="game">The game that the collectibles controller belongs to.</param>
        /// <param name="level">The level that the collectibles controller belongs to.</param>
        /// <returns>The new collectibles controller.</returns>
        public CollectiblesController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _collectibles = new List<Collectible>();

            Level.AddComponents(
                Diamond.Animator = new(Game, Level, "Sprites/Items/Diamond", 20, new(4, 4, 8, 8)),
                SmallHeart.Animator = new(Game, Level, "Sprites/Items/Heart1", 20, new(3, 3, 10, 10)),
                LargeHeart.Animator = new(Game, Level, "Sprites/Items/Heart2", 20, new(1, 2, 14, 14)),
                RespawnAnchor.Animator = new(Game, Level, "Sprites/Items/Star", 20, new(4, 4, 24, 24)),
                Nuke.Animator = new(Game, Level, "Sprites/Items/Gem", 20, new(2, 2, 28, 28))
            );
        }

        /// <summary>
        /// Spawns a diamond at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the diamond at.</param>
        /// <seealso cref="DiamondHollow.Diamond"/>
        public void SpawnDiamond(Point position)
        {
            var diamond = new Diamond(Game, this, position);
            diamond.OnCollect += Level.Player.OnItemCollision;
            _collectibles.Add(diamond);
            Level.AddComponent(diamond);
        }

        /// <summary>
        /// Spawns a cluster of diamonds at the given position, dispersing at a random velocity.
        /// </summary>
        /// <param name="position">The position to spawn the diamonds at.</param>
        /// <param name="count">The number of diamonds to spawn.</param>
        /// <param name="dispersion">The maximum dispersion of the diamonds.</param>
        /// <seealso cref="DiamondHollow.Diamond"/>
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

        /// <summary>
        /// Spawns a small heart at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the heart at.</param>
        /// <seealso cref="DiamondHollow.SmallHeart"/>
        public void SpawnSmallHeart(Point position)
        {
            var heart = new SmallHeart(Game, this, position);
            heart.OnCollect += Level.Player.OnItemCollision;
            _collectibles.Add(heart);
            Level.AddComponent(heart);
        }

        /// <summary>
        /// Spawns a large heart at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the heart at.</param>
        /// <seealso cref="DiamondHollow.LargeHeart"/>
        public void SpawnLargeHeart(Point position)
        {
            var heart = new LargeHeart(Game, this, position);
            heart.OnCollect += Level.Player.OnItemCollision;
            _collectibles.Add(heart);
            Level.AddComponent(heart);
        }

        /// <summary>
        /// Spawns a nuke at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the nuke at.</param>
        /// <seealso cref="DiamondHollow.Nuke"/>
        public void SpawnNuke(Point position)
        {
            var nuke = new Nuke(Game, this, position);
            _collectibles.Add(nuke);
            Level.AddComponent(nuke);
        }

        /// <summary>
        /// Spawns a respawn anchor at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the anchor at.</param>
        /// <seealso cref="DiamondHollow.RespawnAnchor"/>
        public void SpawnRespawnAnchor(Point position)
        {
            var anchor = new RespawnAnchor(Game, this, position);
            _collectibles.Add(anchor);
            Level.AddComponent(anchor);
        }

        /// <summary>
        /// Despawns the given collectible.
        /// </summary>
        /// <param name="collectible">The collectible to despawn.</param>
        internal void Despawn(Collectible collectible)
        {
            _collectibles.Remove(collectible);
            Level.RemoveComponent(collectible);
        }
    }
}