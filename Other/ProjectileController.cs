using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// ProjectileConstructor is used to specify the properties of a projectile.
    /// </summary>
    public struct ProjectileConstructor{
        /// <summary>The entity which fired the projectile.</summary>
        public CollisionBody Owner;
        /// <summary>The origin position of the projectile.</summary>
        public Point Origin;
        /// <summary>The direction of the projectile.</summary>
        public Vector2 Direction;
        /// <summary>The damage strength of the projectile.</summary>
        public int? Damage;
        /// <summary>The speed of the projectile.</summary>
        public float? Speed;
        /// <summary>The size of the projectile.</summary>
        public Point? Size;
        /// <summary>The type of the projectile.</summary>
        public ProjectileType? Type;

        /// <summary>
        /// Creates a new projectile constructor.
        /// </summary>
        /// <param name="owner">The entity which fired the projectile.</param>
        /// <param name="origin">The origin position of the projectile.</param>
        /// <param name="direction">The direction of the projectile.</param>
        /// <param name="damage">The damage strength of the projectile.</param>
        /// <param name="speed">The speed of the projectile.</param>
        /// <param name="size">The size of the projectile.</param>
        /// <param name="type">The type of the projectile.</param>
        public ProjectileConstructor(CollisionBody owner, Point origin, Vector2 direction, int? damage, float? speed, Point? size, ProjectileType? type){
            Owner = owner;
            Origin = origin;
            Direction = direction;
            Damage = damage;
            Speed = speed;
            Size = size;
            Type = type;
        }
    };

    /// <summary>
    /// Handles spawning and despawning of projectiles
    /// </summary>
    public class ProjectileController : DHGameComponent
    {
        /// <summary>
        /// A list of all projectiles.
        /// </summary>
        private readonly List<Projectile> _projectiles;
        /// <summary>
        /// A public copy of the list of all projectiles.
        /// </summary>
        public List<Projectile> Projectiles => _projectiles.ToList();

        /// <summary>
        /// Creates a new projectile controller.
        /// </summary>
        /// <param name="game">The game that this component is a part of.</param>
        /// <param name="level">The level that this component is a part of.</param>
        /// <returns>A new projectile controller.</returns>
        public ProjectileController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _projectiles = new List<Projectile>();

            Level.AddComponents(
                Projectile.Bullet = new(Game, Level, "Sprites/Items/Bullet", 20, new(6, 6, 4, 4)),
                Projectile.Fireball = new(Game, Level, "Sprites/Items/Fireball", 20, new(4, 5, 8, 8))
            );
        }

        /// <summary>
        /// Spawns a projectile.
        /// </summary>
        /// <param name="data">The data used to spawn the projectile.</param>
        /// <see cref="DiamondHollow.Projectile"/>
        public void Spawn(ProjectileConstructor data)
        {
            var projectile = new Projectile(Game, this, data);
            _projectiles.Add(projectile);
            Level.AddComponent(projectile);
        }

        /// <summary>
        /// Despawns a projectile.
        /// </summary>
        /// <param name="projectile">The projectile to despawn.</param>
        internal void Despawn(Projectile projectile)
        {
            _projectiles.Remove(projectile);
            Level.RemoveComponent(projectile);
        }
    }
}