using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    // Handles:
    //     Player movement, shooting
    //     Drawing HUD overlay (hearts, score, etc.)
    //     Player collision with enemy/projectile, collectibles
    public class Player : PhysicsBody
    {
        private enum Countdowns { Invincible, Shoot }

        public bool Invincible  // Player goes invincible after taking damage for a short time
        {
            get => !IsCountdownDone((int)Countdowns.Invincible);
            set => ResetCountdown((int)Countdowns.Invincible);
        }
        public Vector2 Targeting;   // Direction player is aiming, controlled by mouse position
        public int MaxHearts { get; private set; }
        public int Hearts { get; private set; }
        public int Score { get; private set; }

        private Animator Animator;  // Draws the player animations
        private Texture2D _barTexture, _grayHeartTexture, _swordTextures;
        private bool Facing;    // True if player is facing left, false if facing right

        public Player(DiamondHollowGame game, Level level) : base(game, level, new Rectangle(new Point(125, 125), new Point(38))) { }

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

            Level.AddComponent(Animator);   // Ad to scene
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _barTexture = Game.GetTexture("Sprites/UI/Bar1");
            _grayHeartTexture = Game.GetTexture("Sprites/Items/Heart1Empty");
            _swordTextures = Game.GetTexture("Sprites/Items/Swords");
        }

        // Handle player movement and shooting
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

        // Draw a dotted line to indicate player's aiming direction
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