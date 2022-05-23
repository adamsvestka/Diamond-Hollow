using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public enum ProjectileType { Bullet, Fireball }

    public class Projectile : CollisionBody
    {

        private readonly ProjectileController Controller;
        private static readonly Point _defaultSize = new(8);

        private readonly ProjectileType _type;
        public readonly CollisionBody Owner;
        public readonly int Damage;

        public static Animator Bullet, Fireball;

        public Projectile(DiamondHollowGame game, ProjectileController controller, ProjectileConstructor data) : base(game, controller.Level, new Rectangle(data.Origin - (data.Size ?? _defaultSize).Half(), data.Size ?? _defaultSize))
        {
            Controller = controller;
            Owner = data.Owner;
            Damage = data.Damage ?? 10;
            _type = data.Type ?? ProjectileType.Bullet;
            DisableCollisionBox = true;
            Velocity = data.Direction * (data.Speed ?? 10);
            OnCollision += point => Controller.Despawn(this);
        }

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