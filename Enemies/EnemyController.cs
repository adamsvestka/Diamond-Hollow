using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
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
            Level.AddComponent(enemy);
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
            Level.RemoveComponent(enemy);
            if (enemy.Animator != null) Level.RemoveComponent(enemy.Animator);
        }
    }
}