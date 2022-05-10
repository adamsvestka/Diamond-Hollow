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

            Vector2 mask = DisableCollisions ? new(1) : GetCollisionMask();

            _position += mask * Velocity;
            if (mask.X == 0 || mask.Y == 0) OnCollision?.Invoke(Position);

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

        private Vector2 GetCollisionMask()
        {
            Vector2 mask = new(1, 1);

            if (Bounds.OffsetY((int)Velocity.Y).Corners().Any(p => Game.Level.IsWall(p)))
            {
                if (Velocity.Y < 0) _position.Y = (int)Bounds.OffsetY((int)Velocity.Y).Corners(Corner.Bottom).Average(p => p.SnapToGrid().Y) + Game.TileSize;
                else _position.Y = (int)Bounds.OffsetY((int)Velocity.Y).Corners(Corner.Top).Average(p => p.SnapToGrid().Y) - Size.Y;
                Velocity.Y = mask.Y = 0;
            }

            if (Bounds.OffsetX((int)Velocity.X).Corners().Any(p => Game.Level.IsWall(p)))
            {
                if (Velocity.X < 0) _position.X = (int)Bounds.OffsetX((int)Velocity.X).Corners(Corner.Left).Average(p => p.SnapToGrid().X) + Game.TileSize;
                else _position.X = (int)Bounds.OffsetX((int)Velocity.X).Corners(Corner.Right).Average(p => p.SnapToGrid().X) - Size.X;
                Velocity.X = mask.X = 0;
            }

            return mask;
        }
    }
}