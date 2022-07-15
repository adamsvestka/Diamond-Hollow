using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // A component that is affected by gravity and friction
    public class PhysicsBody : CollisionBody
    {
        public float Gravity { get; set; } = 1;     // Objects will have their Y velocity increased by this amount every frame, if not on the ground
        public float Friction { get; set; } = 1;    // Objects with X velocity, which are on the ground, will have their X velocity reduced by this amount each tick

        public PhysicsBody(DiamondHollowGame game, Level level, Rectangle bounds) : base(game, level, bounds) { }

        // These are updated in the Update method, therefore if you want to use them from a derived class's Update method, you should consider first calling base.Update()
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

        // Launch the object in the direction, controls will be disabled until the object settles
        public void Yeet(Vector2 vel)
        {
            Velocity = vel;
            Locked = true;
        }
    }
}