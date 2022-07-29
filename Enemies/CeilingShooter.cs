using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// A less common enemy, is meant to prevent the player from standing still.
    /// Spawns attached to ceilings, cannot move, but shoots projectiles at set intervals.
    /// It will shoot a projectile downward aiming at the player.
    /// Shooting speed &amp; health scale with difficulty.
    /// </summary>
    public class CeilingShooter : Enemy
    {
        /// <summary>
        /// Countdowns for the ceiling shooter's behavior.
        /// </summary>
        private enum Countdowns
        {
            /// <summary>The countdown for when the ceiling shooter will shoot.</summary>
            Shoot,
            /// <summary>The countdown for the ceiling shooter's shooting animation.</summary>
            Animation
        }

        /// <summary>
        /// The size of the ceiling shooter's hitbox.
        /// </summary>
        public static new readonly Point Size = new(50);
        /// <summary>
        /// The direction where the ceiling shooter is shooting.
        /// </summary>
        public Vector2 Targeting;

        /// <summary>
        /// Creates a new ceiling shooter, sets item drop rates, starts countdowns and loads animations.
        /// 
        /// Do not use this constructor directly. Instead, use the <see cref="DiamondHollow.EnemyController.SpawnCeilingShooter"/> method.
        /// </summary>
        /// <param name="game">The game this component is a part of.</param>
        /// <param name="controller">The controller this component is a part of.</param>
        /// <param name="position">The position of the ceiling shooter.</param>
        /// <returns>A new ceiling shooter.</returns>
        public CeilingShooter(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, 30, new Rectangle(position - Size.Half(), Size))
        {
            Targeting = Vector2.Zero;
            Gravity = 0;

            CreateCountdown((int)Countdowns.Animation, (int)(250 / Level.Modifier), true, 40, () => Animator.PlayState("shot"));
            CreateCountdown((int)Countdowns.Shoot, (int)(250 / Level.Modifier), true, 0, () =>
            {
                if (Targeting.Y > 0 || Dead) return;
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

            Animator = new Animator(Game, Level, "Sprites/CeilingShooter/Idle", 10);

            Animator.AddState("shot", Rectangle.Empty, "Sprites/CeilingShooter/Shoot");
            Animator.AddState("hit", Rectangle.Empty, "Sprites/CeilingShooter/Hit");
            Animator.AddState("death", Rectangle.Empty, "Sprites/CeilingShooter/Death");

            Level.AddComponent(Animator);
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Logic to handle ceiling shooter shooting. Ceiling shooters don't move, but shoot a projectile at the player.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Targeting = (Level.Player.Center - Center).ToVector2();
            Targeting.Normalize();
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Draw"/>
        /// <summary>
        /// Draws the ceiling shooter.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Animator.DrawBatch(Bounds.OffsetY(5).ToScreen());

            base.Draw(gameTime);
        }
    }
}