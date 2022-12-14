using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// The most common, easiest to deal with enemy.
    /// Spawns on a platform and moves from side to side in a continuous loop.
    /// Doesn't shoot projectiles, is meant to damage the player by colliding with them.
    /// Movement speed &amp; health scale with difficulty.
    /// </summary>
    public class Slime : Enemy
    {
        /// <summary>
        /// The size of the slime's hitbox.
        /// </summary>
        public static new readonly Point Size = new(48);

        /// <summary>
        /// Creates a new slime, loads animations.
        /// 
        /// Do not use this constructor directly. Instead, use the <see cref="DiamondHollow.EnemyController.SpawnSlime"/> method.
        /// </summary>
        /// <param name="game">The game this component is a part of.</param>
        /// <param name="controller">The controller this component is a part of.</param>
        /// <param name="position">The position of the slime.</param>
        /// <returns>A new slime.</returns>
        public Slime(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, 20, new Rectangle(position - Size.Half(), Size))
        {
            Velocity = new Vector2(3, 0) * Level.Modifier;

            Animator = new(Game, Level, "Sprites/Slime/Idle", 10, new(19, 24, 24, 24));
            Animator.AddState("attack", new(19, 24, 24, 24), "Sprites/Slime/Ability", "Sprites/Slime/AbilityFX");
            Animator.AddState("hit", new(19, 24, 24, 24), "Sprites/Slime/Hit");
            Animator.AddState("death", new(19, 24, 24, 24), "Sprites/Slime/Death");

            Level.AddComponent(Animator);
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Logic to handle the slime's movement. Slimes move on platforms from side to side.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (Level.IsOnGround(Bounds) && !Level.IsOnGround(Bounds.OffsetX((int)(Math.Sign(Velocity.X) * Size.X * 1f)))) Velocity.X = -Velocity.X;    // If on the edge of a platform, reverse direction
            var prev = Velocity;

            base.Update(gameTime);

            if (Velocity == Vector2.Zero && !Dead) Velocity = -prev;    // If it hits a wall, reverse direction
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draws the slime.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Animator.DrawBatch(Bounds.ToScreen(), Velocity.X < 0);

            base.Draw(gameTime);
        }
    }
}