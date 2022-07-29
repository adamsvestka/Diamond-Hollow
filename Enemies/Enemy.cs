using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    /// <summary>
    /// A seperate class so it can have a higher z-index.
    /// </summary>
    public class Healthbar : DHGameComponent
    {
        /// <summary>
        /// The enemy that this healthbar is attached to.
        /// </summary>
        public readonly Enemy Enemy;

        /// <summary>
        /// The healthbar's texture.
        /// </summary>
        public static Texture2D _healthbarFullTexture;

        /// <summary>
        /// Creates a new healthbar.
        /// </summary>
        /// <param name="game">The game this component is a part of.</param>
        /// <param name="level">The level this component is a part of.</param>
        /// <param name="enemy">The enemy this healthbar is attached to.</param>
        /// <returns>The new healthbar.</returns>
        public Healthbar(DiamondHollowGame game, Level level, Enemy enemy) : base(game, level)
        {
            Enemy = enemy;
            DrawOrder = (int)DrawingLayers.Foreground + 1;
            Level.AddComponent(this);
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.LoadContent"/>
        /// <summary>
        /// Loads the healthbar's texture.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            if (_healthbarFullTexture == null) _healthbarFullTexture = Game.GetTexture("Sprites/UI/Healthbar");
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draws the healthbar.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Enemy.Health == Enemy.MaxHealth || Enemy.Dead) return;

            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            int width = (int)(60 * Level.Modifier);     // The visual width of the healthbar
            var bar = new Rectangle(Enemy.Center.X - width / 2, Enemy.Center.Y + Enemy.Size.Y / 2 + 10, width, 10);     // The rectangle that the healthbar will be drawn in
            Level.DrawRectangle(bar.Grow(2), Color.Black);  // Black outline around the healthbar
            Level.DrawRectangle(bar, Color.White);          // A white empty fill
            bar.Width = width * Enemy.Health / Enemy.MaxHealth;     // The filled rectangle of the healthbar
            var cutout = new Rectangle(40, 85, (int)Math.Clamp(bar.Width, 0f, 66f), 5);    // Healthbar texture
            Game.SpriteBatch.Draw(_healthbarFullTexture, bar.ToScreen(), cutout, Color.White);

            Game.SpriteBatch.End();
        }
    }

    /// <summary>
    /// Handles animations, taking damage from projectiles and dropping loot on death.
    /// </summary>
    public class Enemy : PhysicsBody
    {
        /// <summary>
        /// Handles spawning/despawning of enemies.
        /// </summary>
        public EnemyController Controller;
        /// <summary>
        /// The enemy's max health.
        /// </summary>
        public int MaxHealth { get; init; }
        /// <summary>
        /// The enemy's current health.
        /// </summary>
        public int Health { get; private set; }

        /// <summary>
        /// The chance of the enemy dropping a small heart item.
        /// This can be overridden by subclasses and is affected by the difficulty modifier.
        /// </summary>
        protected float HeartDropChance = 0.075f;
        /// <summary>
        /// The number of diamonds the enemy drops when it dies.
        /// This can be overridden by subclasses and is affected by the difficulty modifier.
        /// </summary>
        protected int DiamondDropCount = 5;

        /// <summary>
        /// The enemy's animation controller.
        /// </summary>
        public Animator Animator;
        /// <summary>
        /// If the enemy is dead.
        /// </summary>
        public bool Dead => Health <= 0;
        /// <summary>
        /// Handles drawing the enemy's healthbar.
        /// </summary>
        public Healthbar Healthbar;

        /// <summary>
        /// Creates a new enemy.
        /// </summary>
        /// <param name="game">The game the enemy belongs to.</param>
        /// <param name="controller">The enemy controller the enemy belongs to.</param>
        /// <param name="maxHealth">The enemy's max health.</param>
        /// <param name="bounds">The enemy's bounds.</param>
        /// <returns>The new enemy.</returns>
        /// <remarks>
        /// The enemy's health is set to the max health.
        /// Instead of having a perpetual force applied to them, they receive an initial force and then bounce off of walls.
        /// </remarks>
        public Enemy(DiamondHollowGame game, EnemyController controller, int maxHealth, Rectangle bounds) : base(game, controller.Level, bounds)
        {
            Controller = controller;
            DrawOrder = (int)DrawingLayers.Enemies;
            Healthbar = new Healthbar(game, Level, this);
            Health = MaxHealth = (int)(maxHealth * Level.Modifier / 5f) * 10;
            Friction = 0;   // Instead of having a perpetual force applied to them, I decided to give them an initial force and then let them bounce off of walls
            OnProjectileHit += proj =>
            {
                if (proj.Owner != Level.Player) return;     // Enemies don't damage each other
                if ((Health -= proj.Damage) <= 0)
                {
                    if (Animator?.HasState("death") == true)
                    {
                        // If this enemy has a death animation, stop moving, disable collisions and play it
                        Velocity = new Vector2(Math.Sign(Velocity.X), Math.Sign(Velocity.Y)) / 1000f;
                        Gravity = 0f;
                        DisableCollisionBox = true;
                        Animator.PlayState("death", Die);
                    }
                    else Die();
                }
                else if (Animator?.HasState("hit") == true) Animator.PlayState("hit");
            };
        }

        /// <summary>
        /// Despawn the enemy, spawn some diamonds, maybe drop a heart and show some particles.
        /// </summary>
        private void Die()
        {
            Controller.Despawn(this);
            Level.CollectiblesController.SpawnDiamondCluster(Center, (int)(DiamondDropCount * Level.Modifier), 15);
            if (Game.Chance(HeartDropChance * Level.Modifier)) Level.CollectiblesController.SpawnSmallHeart(Center);
            Level.ParticleController.Spawn(new ParticleConstructor
            {
                Position = Center,
                Color = Color.Red,  // If this enemy doesn't have a texture, default to red particles
                Texture = Animator?.Anim.Textures[0],
                Count = 100,
                DispersionSpeed = 1.5f,
                SpawnRadius = Size.X / 2,
                LifeSpan = 40,
                LifeSpanVariance = 15,
                UsePhysics = true,
            });
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Check for collisions with the player. If so, play an attack animation and deal damage.
        /// </summary>
        /// <seealso cref="DiamondHollow.Player.OnEnemyCollision"/>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Enemy collides with player
            if (Bounds.Intersects(Level.Player.Bounds) && !DisableCollisionBox && !Level.Player.Invincible)
            {
                if (Animator?.HasState("attack") == true) Animator.PlayState("attack");
                Level.Player.OnEnemyCollision(this);
            }
        }
    }
}