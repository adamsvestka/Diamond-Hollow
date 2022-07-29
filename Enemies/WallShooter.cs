using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// The second most common enemy.
    /// Spawns attached to walls, cannot move, but shoots projectiles at set intervals.
    /// On its own, it's nearly harmless, it's meant to restrict the player's movement and paired with other enemies it adds to the chaos.
    /// Shooting speed &amp; health scale with difficulty.
    /// </summary>
    public class WallShooter : Enemy
    {
        /// <summary>
        /// Countdowns for the wall shooter's behavior.
        /// </summary>
        private enum Countdowns
        {
            /// <summary>The countdown for when the wall shooter will shoot.</summary>
            Shoot,
            /// <summary>The countdown for the wall shooter's shooting animation.</summary>
            Animation
        }

        /// <summary>
        /// The size of the wall shooter's hitbox.
        /// </summary>
        public static new readonly Point Size = new(48);
        /// <summary>
        /// The direction where the wall shooter is shooting (facing).
        /// </summary>
        public Vector2 Targeting;

        /// <summary>
        /// Creates a new wall shooter, starts countdowns and loads animations.
        /// 
        /// Do not use this constructor directly. Instead, use the <see cref="DiamondHollow.EnemyController.SpawnWallShooter"/> method.
        /// </summary>
        /// <param name="game">The game this component is a part of.</param>
        /// <param name="controller">The controller this component is a part of.</param>
        /// <param name="position">The position of the wall shooter.</param>
        /// <param name="facingRight">Whether the wall shooter is facing right.</param>
        /// <returns>A new wall shooter.</returns>
        public WallShooter(DiamondHollowGame game, EnemyController controller, Point position, bool facingRight) : base(game, controller, 20, new Rectangle(position - Size.Half(), Size))
        {
            Targeting = new Vector2(facingRight ? 1 : -1, 0);
            Gravity = 0;

            CreateCountdown((int)Countdowns.Animation, (int)(250 / Level.Modifier), true, 120, () => Animator.PlayState("shot"));
            CreateCountdown((int)Countdowns.Shoot, (int)(250 / Level.Modifier), true, 100, () =>
            {
                if (Dead) return;
                Level.ProjectileController.Spawn(new ProjectileConstructor
                {
                    Owner = this,
                    Origin = Center,
                    Direction = Targeting,
                    Size = new Point(24),
                    Speed = 5 * Level.Modifier,
                    Type = ProjectileType.Fireball,
                });
            });

            Animator = new Animator(Game, Level, "Sprites/WallShooter/Idle", 10);

            Animator.AddState("shot", Rectangle.Empty, "Sprites/WallShooter/Shoot");
            Animator.AddState("hit", Rectangle.Empty, "Sprites/WallShooter/Hit");
            Animator.AddState("death", Rectangle.Empty, "Sprites/WallShooter/Death");

            Level.AddComponent(Animator);
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Update"/>
        /// <summary>
        /// Draws the wall shooter.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Animator.DrawBatch(Bounds.OffsetX(Math.Sign(Targeting.X) * -5).ToScreen(), Targeting.X < 0);

            base.Draw(gameTime);
        }
    }
}