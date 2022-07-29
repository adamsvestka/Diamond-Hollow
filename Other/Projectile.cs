using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// The type of the projectile, used to determine which texture to use.
    /// </summary>
    public enum ProjectileType
    {
        /// <summary>Bullets are used by the player.</summary>
        Bullet,
        /// <summary>Fireballs are used by enemies.</summary>
        Fireball
    }

    /// <summary>
    /// A simple collision body, that moves at a constant speed until it hits a wall.
    /// It has a damage strength and cannot damage its owner.
    /// </summary>
    public class Projectile : CollisionBody
    {
        /// <summary>
        /// The controller for the projectile.
        /// </summary>
        private readonly ProjectileController Controller;
        /// <summary>
        /// The default size of projectiles.
        /// </summary>
        private static readonly Point _defaultSize = new(8);

        /// <summary>
        /// The type of the projectile.
        /// </summary>
        private readonly ProjectileType _type;
        /// <summary>
        /// The entity which fired the projectile.
        /// </summary>
        public readonly CollisionBody Owner;
        /// <summary>
        /// The damage strength of the projectile.
        /// </summary>
        public readonly int Damage;

        /// <summary>
        /// An animator for a bullet-type projectile.
        /// </summary>
        public static Animator Bullet;
        /// <summary>
        /// An animator for a fireball-type projectile.
        /// </summary>
        public static Animator Fireball;

        /// <summary>
        /// Creates a new projectile.
        /// 
        /// Do not use this constructor directly. Use <see cref="DiamondHollow.ProjectileController.Spawn"/> instead.
        /// </summary>
        /// <param name="game">The game the projectile belongs to.</param>
        /// <param name="controller">The controller for the projectile.</param>
        /// <param name="data">The data to construct the projectile with.</param>
        /// <returns>The new projectile.</returns>
        public Projectile(DiamondHollowGame game, ProjectileController controller, ProjectileConstructor data) : base(game, controller.Level, new Rectangle(data.Origin - (data.Size ?? _defaultSize).Half(), data.Size ?? _defaultSize))
        {
            Controller = controller;
            DrawOrder = (int)DrawingLayers.Projectiles;
            Owner = data.Owner;
            Damage = data.Damage ?? 10;
            _type = data.Type ?? ProjectileType.Bullet;
            DisableCollisionBox = true;
            Velocity = data.Direction * (data.Speed ?? 10);
            OnCollision += point => Controller.Despawn(this);
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draws the animated projectile, based on its type.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            (_type switch
            {
                ProjectileType.Bullet => Bullet,
                ProjectileType.Fireball => Fireball,
                _ => throw new System.NotImplementedException()
            }).DrawBatch(Bounds.ToScreen());
        }
    }
}