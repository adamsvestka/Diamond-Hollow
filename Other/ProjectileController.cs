using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public record struct ProjectileConstructor(
        CollisionBody Owner,
        Point Origin,
        Vector2 Direction,
        int? Damage,
        float? Speed,
        Point? Size,
        ProjectileType? Type
    );

    public class ProjectileController : DHGameComponent
    {
        private readonly List<Projectile> _projectiles;
        public List<Projectile> Projectiles => _projectiles.ToList();

        public ProjectileController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _projectiles = new List<Projectile>();

            Level.AddComponents(
                Projectile.Bullet = new(Game, Level, "Sprites/Items/Bullet", 20, new(6, 6, 4, 4)),
                Projectile.Fireball = new(Game, Level, "Sprites/Items/Fireball", 20, new(4, 5, 8, 8))
            );
        }

        public void Spawn(ProjectileConstructor data)
        {
            var projectile = new Projectile(Game, this, data);
            _projectiles.Add(projectile);
            Level.AddComponent(projectile);
        }

        internal void Despawn(Projectile projectile)
        {
            _projectiles.Remove(projectile);
            Level.RemoveComponent(projectile);
        }
    }
}