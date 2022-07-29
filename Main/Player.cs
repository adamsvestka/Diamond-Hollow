using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    /// <summary>
    /// Handles:
    /// - Player movement, shooting.
    /// - Drawing HUD overlay (hearts, score, etc.)
    /// - Player collision with enemy/projectile, collectibles.
    /// </summary>
    public class Player : PhysicsBody
    {
        /// <summary>
        /// Player countdowns.
        /// </summary>
        private enum Countdowns
        {
            /// <summary>How long the player is invincible for after being hit.</summary>
            Invincible,
            /// <summary>How long until the player can shoot again after shooting.</summary>
            Shoot
        }

        /// <summary>
        /// Player goes invincible after taking damage for a short time/
        /// </summary>
        public bool Invincible
        {
            get => !IsCountdownDone((int)Countdowns.Invincible);
            set => ResetCountdown((int)Countdowns.Invincible);
        }
        /// <summary>
        /// The direction the player is aiming, controlled by mouse position.
        /// </summary>
        public Vector2 Targeting;
        /// <summary>
        /// The maximum hearts the player can have.
        /// </summary>
        /// <value>Default: 3</value>
        public int MaxHearts { get; private set; }
        /// <summary>
        /// The current hearts the player has.
        /// </summary>
        /// <value>Default: 3</value>
        public int Hearts { get; private set; }
        /// <summary>
        /// The current score the player has.
        /// </summary>
        /// <value>Default: 0</value>
        public int Score { get; private set; }

        /// <summary>
        /// Draws the player animations.
        /// </summary>
        private Animator Animator;
        /// <summary>
        /// A rectangular texture used as a background for the player's score.
        /// </summary>
        private Texture2D _barTexture;
        /// <summary>
        /// An empty heart texture.
        /// </summary>
        private Texture2D _grayHeartTexture;
        /// <summary>
        /// A tileset of swords.
        /// </summary>
        private Texture2D _swordTextures;
        /// <summary>
        /// True if player is facing left, false if facing right.
        /// </summary>
        private bool Facing;

        /// <summary>
        /// Create a new player.
        /// </summary>
        /// <param name="game">The game this component is attached to.</param>
        /// <param name="level">The level this component is attached to.</param>
        /// <returns>The new player.</returns>
        public Player(DiamondHollowGame game, Level level) : base(game, level, new Rectangle(new Point(125, 125), new Point(38))) { }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Initialize"/>
        /// <summary>
        /// - Set the player's health and max health.
        /// - Set the draw order.
        /// - Start countdowns.
        /// - Register callbacks.
        /// - Setup the player's animations, with a random color.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Targeting = Vector2.Zero;
            MaxHearts = Hearts = 3;
            DrawOrder = (int)DrawingLayers.Player;

            // Create non-repeating timers for invincibility and shooting cooldowns
            CreateCountdown((int)Countdowns.Invincible, 90, false, 90);
            CreateCountdown((int)Countdowns.Shoot, 30, false);
            // Projectile hit registers as enemy collision
            OnProjectileHit += OnEnemyCollision;

            // For variety, player will have different colors each time
            string[] Colors = new[] { "Black", "Blue", "Green", "Red", "Yellow" };
            string Color = Game.Choice(Colors);

            // Register player animations, their speeds and cutouts
            Animator = new Animator(Game, Level, $"Sprites/Player/{Color}/Static", 10, new(2, 1, 38, 38));

            Animator.AddState("move", new(2, 2, 38, 38), $"Sprites/Player/{Color}/Run");
            Animator.AddState("jump", new(2, 2, 38, 38), $"Sprites/Player/{Color}/Jump");
            Animator.AddState("death", new(2, 2, 38, 38), $"Sprites/Player/{Color}/Death");

            Level.AddComponent(Animator);   // Add to scene
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.LoadContent"/>
        /// <summary>
        /// Load the player's textures:
        /// - The bar texture for the player's score.
        /// - The empty heart texture.
        /// - The sword textures.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            _barTexture = Game.GetTexture("Sprites/UI/Bar1");
            _grayHeartTexture = Game.GetTexture("Sprites/Items/Heart1Empty");
            _swordTextures = Game.GetTexture("Sprites/Items/Swords");
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Controls for player movement, shooting and animation state.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (!Locked && Hearts > 0)  // Player will be locked for a time after taking damage
            {
                if (Game.KeyboardState.IsKeyDown(Keys.Space) && IsOnGround)
                {
                    Velocity.Y = 21;
                    Animator.PlayState("jump");
                }
                if (Game.KeyboardState.IsKeyDown(Keys.A))
                {
                    Velocity.X = -6;
                    if (IsOnGround && Animator.State == "default") Animator.PlayState("move");
                }
                if (Game.KeyboardState.IsKeyDown(Keys.D))
                {
                    Velocity.X = 6;
                    if (IsOnGround && Animator.State == "default") Animator.PlayState("move");
                }
                if (Game.MouseState.IsButtonDown(MouseButton.Left) && !Invincible && IsCountdownDone((int)Countdowns.Shoot))
                {
                    Level.ProjectileController.Spawn(new ProjectileConstructor
                    {
                        Owner = this,
                        Origin = Center + Targeting.ToPoint(),
                        Direction = Targeting,
                        Speed = 15,
                        Type = ProjectileType.Bullet,
                    });
                    ResetCountdown((int)Countdowns.Shoot);
                }

                if (Velocity.X != 0) Facing = Velocity.X < 0;
                else Animator.PlayState("default");
                if (Game.MouseState.IsButtonDown(MouseButton.Left)) Facing = Targeting.X < 0;
            }

            base.Update(gameTime);

            // Update targeting direction after base class has updated player's position
            Targeting = (Mouse.GetState().Position.ToScreen() - Center).ToVector2();
            Targeting.Normalize();
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// - Draw the player's current animation.
        /// - Draw the player's score, hearts, difficulty and aiming line.
        /// </summary>
        /// <seealso cref="DiamondHollow.Player.DrawCrosshairs"/>
        /// <seealso cref="DiamondHollow.Player.DrawHUD"/>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (Hearts > 0) DrawCrosshairs();
            Point height = new(57);     // Player is taller than hitbox, this is only visual
            Animator.Draw(new Rectangle(Center - new Point(height.X, Size.Y).Half(), height).ToScreen(), Facing, Invincible || (Hearts == 0) ? 0.5f : 1f);
            DrawHUD();

            Game.SpriteBatch.End();
        }

        /// <summary>
        /// Draw a dotted line to indicate player's aiming direction.
        /// </summary>
        /// <seealso cref="DiamondHollow.Player.Draw"/>
        private void DrawCrosshairs()
        {
            var center = Bounds.Center.ToVector2();
            var size = new Vector2(4);
            var spacing = 20;
            var count = (Game.WindowWidth + Game.WindowHeight) / spacing;
            for (int i = 0; i < count; i++)
            {
                var dot = center + Targeting * i * spacing;
                Level.DrawRectangle(new Rectangle((dot - size / 2).ToPoint(), size.ToPoint()), Color.Red * 0.5f);
            }
        }

        /// <summary>
        /// Draw the player's health, difficulty and score.
        /// - Health is drawn as red hearts in the top left corner.
        /// - Difficulty is drawn as swords in the top left corner, under the health.
        /// - Score is drawn as a number next a diamond in the bottom left corner.
        /// </summary>
        /// <seealso cref="DiamondHollow.Player.Draw"/>
        private void DrawHUD()
        {
            var heartPosition = new Rectangle(new Point(13, 7), SmallHeart.Size);
            for (int i = 0; i < MaxHearts; i++)
            {
                if (i < Hearts) SmallHeart.Animator.Draw(heartPosition);  // Full hearts are red and animated
                else Game.SpriteBatch.Draw(_grayHeartTexture, heartPosition, new Rectangle(3, 3, 10, 10), Color.White);   // Empty hearts are gray
                heartPosition.X += heartPosition.Width + 7;
            }
            var swordPosition = new Rectangle(new Point(13, 7 + SmallHeart.Size.Y + 7), new Point(32));
            for (int i = 0; i < (int)Level.Difficulty; i++)
            {
                Game.SpriteBatch.Draw(_swordTextures, swordPosition, new Rectangle(0 * 16, 9 * 16, 16, 16), Color.White);     // Draw swords
                swordPosition.X += heartPosition.Width + 7;
            }
            if (Level.Difficulty % 1 > 0.66f) Game.SpriteBatch.Draw(_swordTextures, swordPosition, new Rectangle(2 * 16, 9 * 16, 16, 16), Color.White);   // Draw 2/3 partial sword
            else if (Level.Difficulty % 1 > 0.33f) Game.SpriteBatch.Draw(_swordTextures, swordPosition, new Rectangle(2 * 16, 1 * 16, 16, 16), Color.White);  // Draw 1/3 partial sword

            // Draw score, diamond is animated
            var diamondPosition = new Rectangle(new Point(25, Game.WindowHeight - Diamond.Size.Y - 13), Diamond.Size);
            Game.SpriteBatch.Draw(_barTexture, new Rectangle(4, Game.WindowHeight - 42, 164, 36), Color.White * 0.9f);
            Diamond.Animator.Draw(diamondPosition);
            Game.SpriteBatch.DrawString(Game.Menlo, $"{Score}", new Vector2(diamondPosition.Center.X + 50, diamondPosition.Top + 2), Color.Black);
        }


        /// <summary>
        /// Play the player's death animation, spawn particles, move the player to the last checkpoint and smoothly scroll the camera to the player.
        /// </summary>
        /// <seealso cref="DiamondHollow.Camera.Scroll"/>
        public void Die()
        {
            Animator.PlayState("death", () =>
            {
                Level.ParticleController.Spawn(new ParticleConstructor
                {
                    Position = Center,
                    Texture = Animator?.Anim.Textures[0],   // Particles' colors are sampled from the texture of the current player's animation
                    Count = 100,
                    DispersionSpeed = 1.5f,
                    SpawnRadius = 10,
                    LifeSpan = 40,
                    LifeSpanVariance = 15,
                    UsePhysics = true,
                });
                Position = Level.Spawnpoint - Size.Half();
                Velocity = Vector2.Zero;
                Level.Camera.Scroll(Center.Y, 120, () => Hearts = MaxHearts);   // Scroll player (now at last checkpoint) into view
            });
        }

        /// <summary>
        /// Player gets hit by a projectile, lower health, gain invincibility and knock the player back.
        /// If the player is out of hearts, the player dies.
        /// </summary>
        /// <param name="enemy"></param>
        public void OnEnemyCollision(CollisionBody enemy)
        {
            if (Invincible || Hearts == 0) return;
            if (--Hearts == 0)
            {
                Die();
                return;
            }

            Invincible = true;

            // After taking damage, the player is knocked away from the source of damage
            Vector2 dir = (Center - enemy.Center).ToVector2();
            dir.X = Math.Sign(dir.X);
            dir.Y = dir.Y == 0 ? 1 : Math.Sign(dir.Y);
            Yeet(dir * 10);
        }

        /// <summary>
        /// Player collects a item.
        /// - Diamonds are collected to increase score.
        /// - Small hearts are collected to heal one heart, but only if the player has less than max hearts.
        /// - Big hearts are collected to heal all hearts and increase the player's max hearts by one.
        /// </summary>
        /// <param name="item"></param>
        public void OnItemCollision(Collectible item)
        {
            switch (item)
            {
                case Diamond:
                    Score++;
                    break;
                case SmallHeart:
                    if (Hearts < MaxHearts) Hearts++;
                    break;
                case LargeHeart:
                    Hearts = ++MaxHearts;
                    break;
            }
        }
    }
}