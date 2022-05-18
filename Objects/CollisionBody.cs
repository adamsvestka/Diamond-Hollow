using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class CollisionBody : DHGameComponent
    {
        protected Vector2 _position;
        public Vector2 Velocity;
        public Point Size;
        public bool DisableCollisions = false;
        public bool DisableCollisionBox = false;

        public Point Position { get => _position.ToPoint(); set => _position = value.ToVector2(); }
        public Rectangle Bounds => new(Position, Size);
        public Point Center => Bounds.Center;

        public event Action<Point> OnCollision;
        public event Action<Projectile> OnProjectileHit;

        public CollisionBody(DiamondHollowGame game, Level level, Rectangle bounds) : base(game, level)
        {
            Position = bounds.Location;
            Size = bounds.Size;
            Velocity = new Vector2(0, 0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Vector2 mask = DisableCollisions ? Vector2.One : GetCollisionMask(_position, ref Velocity);

            if (mask.X == 0 || mask.Y == 0) OnCollision?.Invoke(Position);
            _position += Velocity;
            Velocity *= mask;

            if (!DisableCollisions && !DisableCollisionBox)
            {
                foreach (var proj in Level.ProjectileController.Projectiles)
                {
                    if (proj.Owner != this && Bounds.Intersects(proj.Bounds))
                    {
                        OnProjectileHit?.Invoke(proj);
                        proj.OnCollision?.Invoke(Position);
                    }
                }
            }
        }

        private Vector2 GetCollisionMask(in Vector2 pos, ref Vector2 vel, int depth = 3)
        {
            Vector2 a = pos, b = pos + vel;
            var collides = (Vector2 p) => new Rectangle(p.ToPoint(), Size).Corners().Any(p => Level.IsWall(p));
            if (!collides(b)) return Vector2.One;

            Vector2 c = (a + b) / 2;
            while ((b - a).Length() > 0.5f)
            {
                if (!collides(c)) a = c;
                else b = c;
                c = (a + b) / 2;
            }

            bool collisionX = collides(a + new Vector2(Math.Sign(vel.X), 0));
            bool collisionY = collides(a + new Vector2(0, Math.Sign(vel.Y)));

            var mask = new Vector2(collisionX ? 0 : 1, collisionY ? 0 : 1);
            if (depth > 0)
            {
                if (collisionX)
                {
                    vel.X = 0;
                    mask.Y = GetCollisionMask(a, ref vel, depth - 1).Y;
                    vel.X = a.X - pos.X;
                }
                if (collisionY)
                {
                    vel.Y = 0;
                    mask.X = GetCollisionMask(a, ref vel, depth - 1).X;
                    vel.Y = a.Y - pos.Y;
                }
            }

            return mask;
        }
    }
}