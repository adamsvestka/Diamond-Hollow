using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class PhysicsBody : CollisionBody
    {
        public float Gravity { get; set; } = 1;
        public float Friction { get; set; } = 1;

        public PhysicsBody(DiamondHollowGame game, Level level, Rectangle bounds) : base(game, level, bounds) { }

        public bool IsOnGround { get; private set; }
        public bool Locked { get; private set; }

        public override void Update(GameTime gameTime)
        {
            if (!DisableCollisions)
            {
                if (IsOnGround = Level.IsOnGround(Bounds))
                {
                    if (Math.Abs(Velocity.X -= Math.Sign(Velocity.X) * Friction) < Friction) Velocity.X = 0;
                }
                else Velocity.Y -= Gravity;
            }

            if (Locked && Velocity == Vector2.Zero) Locked = false;

            base.Update(gameTime);
        }

        public void Yeet(Vector2 vel)
        {
            Velocity = vel;
            Locked = true;
        }
    }
}