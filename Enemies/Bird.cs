using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// The rarest, most difficult to deal with enemy.
    /// Spawns in the air and flies from wall to wall in a continuous loop.
    /// After a certain difficulty is reached, when the player walks under it, it will shoot a projectile downward.
    /// Also has a lot of health and a high chance of dropping a heart.
    /// You can pretty easily avoid it, but it's a way to replenish health, though risky.
    /// Movement speed, shooting speed &amp; health scale with difficulty.
    /// </summary>
    public class Bird : Enemy
    {
        /// <summary>
        /// Countdowns for the bird's behavior.
        /// </summary>
        private enum Countdowns
        {
            /// <summary>The countdown for when the bird will shoot.</summary>
            Shoot
        }

        /// <summary>
        /// The size of the bird's hitbox.
        /// </summary>
        public static new readonly Point Size = new(68, 48);
        /// <summary>
        /// The direction where the bird is shooting.
        /// </summary>
        public Vector2 Targeting;

        /// <summary>
        /// Creates a new bird, sets item drop rates, starts countdowns and loads animations.
        /// 
        /// Do not use this constructor directly. Instead, use the <see cref="DiamondHollow.EnemyController.SpawnBird"/> method.
        /// </summary>
        /// <param name="game">The game this component is a part of.</param>
        /// <param name="controller">The controller this component is a part of.</param>
        /// <param name="position">The position of the bird.</param>
        /// <returns>A new bird.</returns>
        public Bird(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, 40, new Rectangle(position - Size.Half(), Size))
        {
            Velocity = new Vector2(4.5f, 0) * Level.Modifier;
            Gravity = 0;
            Targeting = new Vector2(0, -1);
            HeartDropChance = 0.7f;
            DiamondDropCount = 10;

            CreateCountdown((int)Countdowns.Shoot, (int)(150 / Level.Modifier), false);

            Animator = new Animator(Game, Level, "Sprites/Bird/Idle", 10, new(4, 4, 34, 34));

            Animator.AddState("death", new(4, 4, 24, 24), "Sprites/Bird/Death");

            Level.AddComponent(Animator);
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Logic to handle bird movement and shooting. Birds fly from wall to wall in a continuous loop and shoot a projectile at the player.
        /// </summary>
        /// <remarks>
        /// Only shoots from difficulty 5 and up.
        /// </remarks>
        /// <seealso cref="DiamondHollow.Bird.CheckForPlayer"/>
        public override void Update(GameTime gameTime)
        {
            var prev = Velocity;

            base.Update(gameTime);

            if (Velocity == Vector2.Zero) Velocity = -prev;     // If it hits a wall, reverse direction

            if (Level.Difficulty > 5 && IsCountdownDone((int)Countdowns.Shoot) && CheckForPlayer())
            {
                Level.ProjectileController.Spawn(new ProjectileConstructor
                {
                    Owner = this,
                    Origin = Center,
                    Direction = Targeting,
                    Size = new Point(32),
                    Speed = 6.5f * Level.Modifier,
                    Type = ProjectileType.Fireball,
                });
                ResetCountdown((int)Countdowns.Shoot);
            }
        }

        /// <summary>
        /// Check if a player is under the bird. Used to determine if the bird should shoot.
        /// </summary>
        /// <returns>True if a player is under the bird, false otherwise.</returns>
        private bool CheckForPlayer()
        {
            int xdiff = Center.X - Level.Player.Center.X;
            int ydiff = Center.Y - Level.Player.Center.Y;
            return Math.Abs(xdiff) < Level.Player.Size.X / 2 && 0 < ydiff && ydiff < 12 * Game.TileSize;
        }

        // <inheritdoc cref="DiamondHollow.Bird.Draw"/>
        /// <summary>
        /// Draws the bird.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Animator.DrawBatch(new Rectangle(Center - new Point(Size.X).Half(), new Point(Size.X)).ToScreen(), Velocity.X < 0);

            base.Draw(gameTime);
        }
    }
}