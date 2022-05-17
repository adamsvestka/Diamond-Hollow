using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    public class Player : PhysicsBody
    {
        private enum Countdowns { Invincible, Shoot }

        public bool Invincible
        {
            get => !IsCountdownDone((int)Countdowns.Invincible);
            set => ResetCountdown((int)Countdowns.Invincible);
        }
        public Vector2 Targeting;
        public int MaxHearts { get; private set; }
        public int Hearts { get; private set; }
        public int Score { get; private set; }

        private Texture2D _barTexture1, _barTexture2, _grayHeartTexture;

        public Player(DiamondHollowGame game, Level level) : base(game, level, new Rectangle(new Point(125, 125), new Point(36))) { }

        public override void Initialize()
        {
            base.Initialize();

            Targeting = Vector2.Zero;
            MaxHearts = Hearts = 3;

            CreateCountdown((int)Countdowns.Invincible, 90, false, 90);
            CreateCountdown((int)Countdowns.Shoot, 30, false);
            OnProjectileHit += OnEnemyCollision;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _barTexture1 = Game.Content.Load<Texture2D>("Sprites/Bar1");
            _barTexture2 = Game.Content.Load<Texture2D>("Sprites/Bar2");
            _grayHeartTexture = Game.Content.Load<Texture2D>("Sprites/Heart1Empty");
        }

        public override void Update(GameTime gameTime)
        {
            if (!Locked && Hearts > 0)
            {
                if (Game.KeyboardState.IsKeyDown(Keys.Space) && IsOnGround) Velocity.Y = 21;
                if (Game.KeyboardState.IsKeyDown(Keys.A)) Velocity.X = -6;
                if (Game.KeyboardState.IsKeyDown(Keys.D)) Velocity.X = 6;
                if (Game.MouseState.IsButtonDown(MouseButton.Left) && !Invincible && IsCountdownDone((int)Countdowns.Shoot))
                {
                    Level.ProjectileController.Spawn(new ProjectileConstructor
                    {
                        Owner = this,
                        Origin = Center + Targeting.ToPoint(),
                        Direction = Targeting,
                        Color = Color.Purple,
                        Speed = 15,
                    });
                    ResetCountdown((int)Countdowns.Shoot);
                }
            }

            base.Update(gameTime);

            Targeting = (Mouse.GetState().Position.ToScreen() - Center).ToVector2();
            Targeting.Normalize();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (Hearts > 0) DrawCrosshairs();
            Level.DrawRectangle(Bounds, Color.Blue * (Invincible || Hearts == 0 ? 0.5f : 1f));
            DrawHUD();
            // DrawDebug();

            Game.SpriteBatch.End();
        }

        private void DrawDebug()
        {
            Game.SpriteBatch.DrawString(Game.Menlo, $"Difficulty: {Level.Difficulty}", new Vector2(10, 50), Color.LightBlue);
            Game.SpriteBatch.DrawString(Game.Menlo, $"Modifier: {Level.Modifier}", new Vector2(10, 70), Color.LightBlue);
        }

        private void DrawCrosshairs()
        {
            var center = Bounds.Center.ToVector2();
            var size = new Vector2(4);
            var spacing = 20;
            var count = (Game.WindowWidth + Game.WindowHeight) / spacing;
            for (int i = 0; i < count; i++)
            {
                var dot = center + Targeting * i * spacing;
                Level.DrawRectangle(new Rectangle((dot - size / 2).ToPoint(), size.ToPoint()), Color.Red);
            }
        }

        private void DrawHUD()
        {
            var heart = new Rectangle(new Point(13, 7), SmallHeart.Size);
            // Game.SpriteBatch.Draw(_barTexture2, new Rectangle(4, 6, 164, 36), Color.White);
            for (int i = 0; i < MaxHearts; i++)
            {
                if (i < Hearts) SmallHeart.Animator.Draw(heart);
                else Game.SpriteBatch.Draw(_grayHeartTexture, heart, new Rectangle(3, 3, 10, 10), Color.White);
                heart.X += heart.Width + 7;
            }

            var diamond = new Rectangle(new Point(25, Game.WindowHeight - Diamond.Size.Y - 13), Diamond.Size);
            Game.SpriteBatch.Draw(_barTexture1, new Rectangle(4, Game.WindowHeight - 42, 164, 36), Color.White * 0.9f);
            Diamond.Animator.Draw(diamond);
            Game.SpriteBatch.DrawString(Game.Menlo, $"{Score}", new Vector2(diamond.Center.X + 50, diamond.Top + 2), Color.Black);
        }

        public void Die()
        {
            Level.ParticleController.Spawn(new ParticleConstructor
            {
                Position = Center,
                Color = Color.Blue,
                Count = 100,
                DispersionSpeed = 1.5f,
                SpawnRadius = 10,
                LifeSpan = 40,
                LifeSpanVariance = 15,
                UsePhysics = true,
            });
            Position = Level.Spawnpoint - Size.Half();
            Velocity = Vector2.Zero;
            Level.Camera.Scroll(Center.Y, 120, () => Hearts = MaxHearts);
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