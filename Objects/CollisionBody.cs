using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// Handles collision with either the map or projectiles.
    /// </summary>
    public class CollisionBody : DHGameComponent
    {
        /// <summary>
        /// The object's position.
        /// </summary>
        protected Vector2 _position;
        /// <summary>
        /// The object's velocity.
        /// </summary>
        public Vector2 Velocity;
        /// <summary>
        /// The object's size.
        /// </summary>
        public Point Size;
        /// <summary>
        /// If true, will not collide with anything
        /// </summary>
        public bool DisableCollisions = false;
        /// <summary>
        /// If true, will not collide with projectiles
        /// </summary>
        public bool DisableCollisionBox = false;

        /// <summary>
        /// A public wrapper for <see cref="DiamondHollow.CollisionBody._position"/> property.
        /// Converts to <see cref="Microsoft.Xna.Framework.Point"/>.
        /// </summary>
        public Point Position { get => _position.ToPoint(); set => _position = value.ToVector2(); }
        /// <summary>
        /// The object's bounding box.
        /// </summary>
        public Rectangle Bounds => new(Position, Size);
        /// <summary>
        /// The object's center.
        /// </summary>
        public Point Center => Bounds.Center;

        /// <summary>
        /// Fires when the object collides with the map.
        /// </summary>
        public event Action<Point> OnCollision;
        /// <summary>
        /// Fires when the object collides with a projectile.
        /// </summary>
        public event Action<Projectile> OnProjectileHit;

        /// <summary>
        /// Create a new collision body.
        /// </summary>
        /// <param name="game">The game this component is attached to.</param>
        /// <param name="level">The level this component is attached to.</param>
        /// <param name="bounds">The bounding box of the object.</param>
        /// <returns>The new collision body.</returns>
        public CollisionBody(DiamondHollowGame game, Level level, Rectangle bounds) : base(game, level)
        {
            Position = bounds.Location;
            Size = bounds.Size;
            Velocity = new Vector2(0, 0);
        }

        // <inheritdoc cref="DiamondHollow.DHGameComponent.Update"/>
        /// <summary>
        /// Moves the object according to its velocity, handles collisions with the map.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Mask is used as a tuple of bools representing if object collided on given axis
            // 1 = no collision, 0 = collision
            Vector2 mask = DisableCollisions ? Vector2.One : GetCollisionMask(_position, ref Velocity);     // Velocity may be modified to prevent overlap. Yes, this is a mess.

            if (mask.X == 0 || mask.Y == 0) OnCollision?.Invoke(Position);
            _position += Velocity;
            Velocity *= mask;

            if (!DisableCollisions && !DisableCollisionBox)
            {
                // Check for collisions with projectiles and raise the appropriate events
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

        // PAIN. Don't even ask me how this works.
        // Edit at your own risk. I take no responsibility for any physical, mental, or emotional damages you may incur.
        /// <summary>
        /// Checks for collisions with the map.
        /// </summary>
        /// <param name="pos">The position of the object.</param>
        /// <param name="vel">The velocity of the object.</param>
        /// <param name="depth">*ignore this*</param>
        /// <returns>The mask representing the collision.
        /// 
        /// 1 = no collision, 0 = collision</returns>
        private Vector2 GetCollisionMask(in Vector2 pos, ref Vector2 vel, int depth = 3)    // depth is here to prevent infitite recursion... because I couldn't figure out why it wouldn't stop ¯\_(ツ)_/¯
        {
            // Do something like binary search because it's not that expensive and I spent way too much time trying to figure out how to actually calculate the exact position accounting for all possible platform shapes.
            Vector2 a = pos, b = pos + vel;
            var collides = (Vector2 p) => new Rectangle(p.ToPoint(), Size).Corners().Any(p => Level.IsWall(p));
            if (!collides(b)) return Vector2.One;   // Return early if no collision

            // B.S.
            Vector2 c = (a + b) / 2;
            while ((b - a).Length() > 0.5f)
            {
                if (!collides(c)) a = c;
                else b = c;
                c = (a + b) / 2;
            }

            // Offset the collision box *slightly* in X and Y directions and figure out if it hit anything
            bool collisionX = collides(a + new Vector2(Math.Sign(vel.X), 0));
            bool collisionY = collides(a + new Vector2(0, Math.Sign(vel.Y)));

            // So basically, if an object hits a wall I don't want it to lose all momentum, instead I want it to keep it's velocity in the unaffected direction.
            // BUT it could also hit something in that direction later on, which the previous code would discover, so I just call this function again with the new position and velocity only in the unobstructed direction.
            var mask = new Vector2(collisionX ? 0 : 1, collisionY ? 0 : 1);
            if (depth > 0)
            {
                if (collisionX)
                {
                    vel.X = 0;
                    mask.Y = GetCollisionMask(a, ref vel, depth - 1).Y;
                    vel.X = a.X - pos.X;    // Move in the direction only to the point of collision
                }
                if (collisionY)
                {
                    vel.Y = 0;
                    mask.X = GetCollisionMask(a, ref vel, depth - 1).X;
                    vel.Y = a.Y - pos.Y;    // -||-
                }
            }

            return mask;    // This is here because bad design, I guess.
        }
    }
}