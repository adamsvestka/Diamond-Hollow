using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class PhysicsBody : CollisionBody
    {
        public float Gravity { get; set; } = 1;
        public float Friction { get; set; } = 1;

        public PhysicsBody(DiamondHollowGame game, Rectangle bounds) : base(game, bounds) { }

        public bool IsOnGround { get; private set; }

        public override void Update(GameTime gameTime)
        {
            if (!DisableCollisions)
            {
                if (IsOnGround = Game.Level.IsOnGround(BoundingBox))
                {
                    if (Math.Abs(Velocity.X -= Math.Sign(Velocity.X) * Friction) < Friction) Velocity.X = 0;
                }
                else Velocity.Y -= Gravity;
            }

            base.Update(gameTime);
        }
    }
}