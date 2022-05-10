using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public record struct ProjectileConstructor(
        CollisionBody Owner,
        Point Origin,
        Vector2 Direction,
        float? Speed,
        Point? Size,
        Color? Color
    );

    public class ProjectileController : DHGameComponent
    {
        private readonly List<Projectile> _projectiles;
        public List<Projectile> Projectiles => _projectiles.ToList();

        public ProjectileController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _projectiles = new List<Projectile>();
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