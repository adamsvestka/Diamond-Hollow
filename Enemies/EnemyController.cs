using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // A unified place to spawn all types of enemies
    // Doesn't really do much here, but I used this design for other systems in the game, so I used it here as well
    public class EnemyController : DHGameComponent
    {
        private readonly List<Enemy> _enemies;

        public EnemyController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _enemies = new List<Enemy>();
        }

        public void SpawnSlime(Point position)
        {
            var enemy = new Slime(Game, this, position);
            _enemies.Add(enemy);
            Level.AddComponents(enemy);
        }

        public void SpawnWallShooter(Point position, bool facingRight)
        {
            var enemy = new WallShooter(Game, this, position, facingRight);
            _enemies.Add(enemy);
            Level.AddComponent(enemy);
        }

        public void SpawnCeilingShooter(Point position)
        {
            var enemy = new CeilingShooter(Game, this, position);
            _enemies.Add(enemy);
            Level.AddComponent(enemy);
        }

        public void SpawnSpider(Point position)
        {
            var enemy = new Spider(Game, this, position);
            _enemies.Add(enemy);
            Level.AddComponent(enemy);
        }

        public void SpawnBird(Point position)
        {
            var enemy = new Bird(Game, this, position);
            _enemies.Add(enemy);
            Level.AddComponent(enemy);
        }

        internal void Despawn(Enemy enemy)
        {
            _enemies.Remove(enemy);
            Level.RemoveComponents(enemy, enemy.Healthbar);
            if (enemy.Animator != null) Level.RemoveComponent(enemy.Animator);
        }
    }
}