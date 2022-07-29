using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// A component that is affected by gravity and friction.
    /// </summary>
    public class PhysicsBody : CollisionBody
    {
        /// <summary>
        /// Objects will have their vertical velocity increased by this amount every tick, if not on the ground.
        /// </summary>
        /// <value>Default: 1</value>
        public float Gravity { get; set; } = 1;
        /// <summary>
        /// Objects with horizontal velocity, which are on the ground, will have their horizontal velocity reduced by this amount each tick.
        /// </summary>
        /// <value>Default: 1</value>
        public float Friction { get; set; } = 1;

        /// <summary>
        /// Create a new physics body.
        /// </summary>
        /// <param name="game">The game this component is attached to.</param>
        /// <param name="level">The level this component is attached to.</param>
        /// <param name="bounds">The bounding box of the object.</param>
        /// <returns>The new physics body.</returns>
        public PhysicsBody(DiamondHollowGame game, Level level, Rectangle bounds) : base(game, level, bounds) { }

        /// <summary>
        /// Whether or not the object is on the ground.
        /// </summary>
        /// <summary>
        /// These are updated in the Update method, therefore if you want to use them from a derived class's Update method, you should consider first calling <see cref="DiamondHollow.PhysicsBody.Update"/>.
        /// </summary>
        public bool IsOnGround { get; private set; }
        /// <summary>
        /// Whether or not the object is being affected by another force and cannot move on its own.
        /// </summary>
        /// <summary>
        /// These are updated in the Update method, therefore if you want to use them from a derived class's Update method, you should consider first calling <see cref="DiamondHollow.PhysicsBody.Update"/>.
        /// </summary>
        public bool Locked { get; private set; }

        // <inheritdoc cref="DiamondHollow.CollisionBody.Update"/>
        /// <summary>
        /// Updates <see cref="DiamondHollow.PhysicsBody.IsOnGround"/> and <see cref="DiamondHollow.PhysicsBody.Locked"/>.
        /// </summary>
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

        /// <summary>
        /// Launch the object in the direction, controls will be disabled until the object settles.
        /// </summary>
        /// <param name="vel">The velocity to launch the object with.</param>
        public void Yeet(Vector2 vel)
        {
            Velocity = vel;
            Locked = true;
        }
    }
}