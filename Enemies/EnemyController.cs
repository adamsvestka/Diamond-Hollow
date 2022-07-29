using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// A unified place to spawn all types of enemies.
    /// Doesn't really do much here, but I used this design for other systems in the game, so I used it here as well.
    /// </summary>
    public class EnemyController : DHGameComponent
    {
        /// <summary>
        /// A list of all enemies in the game.
        /// </summary>
        private readonly List<Enemy> _enemies;

        /// <summary>
        /// Creates a new EnemyController.
        /// </summary>
        /// <param name="game">The game this component is a part of.</param>
        /// <param name="level">The level this component is a part of.</param>
        /// <returns>A new EnemyController.</returns>
        public EnemyController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _enemies = new List<Enemy>();
        }

        /// <summary>
        /// Spawns a slime at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the slime at.</param>
        public void SpawnSlime(Point position)
        {
            var enemy = new Slime(Game, this, position);
            _enemies.Add(enemy);
            Level.AddComponents(enemy);
        }

        /// <summary>
        /// Spawn a wall shooter at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the wall shooter at.</param>
        /// <param name="facingRight">Whether the wall shooter is facing right.</param>
        public void SpawnWallShooter(Point position, bool facingRight)
        {
            var enemy = new WallShooter(Game, this, position, facingRight);
            _enemies.Add(enemy);
            Level.AddComponent(enemy);
        }

        /// <summary>
        /// Spawn a ceiling shooter at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the ceiling shooter at.</param>
        public void SpawnCeilingShooter(Point position)
        {
            var enemy = new CeilingShooter(Game, this, position);
            _enemies.Add(enemy);
            Level.AddComponent(enemy);
        }

        /// <summary>
        /// Spawn a spider at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the spider at.</param>
        public void SpawnSpider(Point position)
        {
            var enemy = new Spider(Game, this, position);
            _enemies.Add(enemy);
            Level.AddComponent(enemy);
        }

        /// <summary>
        /// Spawn a bird at the given position.
        /// </summary>
        /// <param name="position">The position to spawn the bird at.</param>
        public void SpawnBird(Point position)
        {
            var enemy = new Bird(Game, this, position);
            _enemies.Add(enemy);
            Level.AddComponent(enemy);
        }

        /// <summary>
        /// Despawns the given enemy.
        /// </summary>
        /// <param name="enemy">The enemy to despawn.</param>
        internal void Despawn(Enemy enemy)
        {
            _enemies.Remove(enemy);
            Level.RemoveComponents(enemy, enemy.Healthbar);
            if (enemy.Animator != null) Level.RemoveComponent(enemy.Animator);
        }
    }
}