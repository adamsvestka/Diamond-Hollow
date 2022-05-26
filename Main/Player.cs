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

        private Animator Animator;
        private Texture2D _barTexture, _grayHeartTexture, _swordTextures;
        private bool Facing;

        public Player(DiamondHollowGame game, Level level) : base(game, level, new Rectangle(new Point(125, 125), new Point(38))) { }

        public override void Initialize()
        {
            base.Initialize();

            Targeting = Vector2.Zero;
            MaxHearts = Hearts = 3;
            DrawOrder = (int)DrawingLayers.Player;

            CreateCountdown((int)Countdowns.Invincible, 90, false, 90);
            CreateCountdown((int)Countdowns.Shoot, 30, false);
            OnProjectileHit += OnEnemyCollision;

            string[] Colors = new[] { "Black", "Blue", "Green", "Red", "Yellow" };
            string Color = Game.Choice(Colors);

            Animator = new Animator(Game, Level, $"Sprites/Player/{Color}/Static", 10, new(2, 1, 38, 38));

            Animator.AddState("move", new(2, 2, 38, 38), $"Sprites/Player/{Color}/Run");
            Animator.AddState("jump", new(2, 2, 38, 38), $"Sprites/Player/{Color}/Jump");
            Animator.AddState("death", new(2, 2, 38, 38), $"Sprites/Player/{Color}/Death");

            Level.AddComponent(Animator);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _barTexture = Game.GetTexture("Sprites/UI/Bar1");
            _grayHeartTexture = Game.GetTexture("Sprites/Items/Heart1Empty");
            _swordTextures = Game.GetTexture("Sprites/Items/Swords");
        }

        public override void Update(GameTime gameTime)
        {
            if (!Locked && Hearts > 0)
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

            Targeting = (Mouse.GetState().Position.ToScreen() - Center).ToVector2();
            Targeting.Normalize();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (Hearts > 0) DrawCrosshairs();
            Point s = new(57);
            Animator.Draw(new Rectangle(Center - new Point(s.X, Size.Y).Half(), s).ToScreen(), Facing, Invincible || (Hearts == 0) ? 0.5f : 1f);
            DrawHUD();

            Game.SpriteBatch.End();
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
                Level.DrawRectangle(new Rectangle((dot - size / 2).ToPoint(), size.ToPoint()), Color.Red * 0.5f);
            }
        }

        private void DrawHUD()
        {
            var heart = new Rectangle(new Point(13, 7), SmallHeart.Size);
            for (int i = 0; i < MaxHearts; i++)
            {
                if (i < Hearts) SmallHeart.Animator.Draw(heart);
                else Game.SpriteBatch.Draw(_grayHeartTexture, heart, new Rectangle(3, 3, 10, 10), Color.White);
                heart.X += heart.Width + 7;
            }
            var sword = new Rectangle(new Point(13, 7 + SmallHeart.Size.Y + 7), new Point(32));
            for (int i = 0; i < (int)Level.Difficulty; i++)
            {
                Game.SpriteBatch.Draw(_swordTextures, sword, new Rectangle(0 * 16, 9 * 16, 16, 16), Color.White);
                sword.X += heart.Width + 7;
            }
            if (Level.Difficulty % 1 > 0.66f) Game.SpriteBatch.Draw(_swordTextures, sword, new Rectangle(2 * 16, 9 * 16, 16, 16), Color.White);
            else if (Level.Difficulty % 1 > 0.33f) Game.SpriteBatch.Draw(_swordTextures, sword, new Rectangle(2 * 16, 1 * 16, 16, 16), Color.White);

            var diamond = new Rectangle(new Point(25, Game.WindowHeight - Diamond.Size.Y - 13), Diamond.Size);
            Game.SpriteBatch.Draw(_barTexture, new Rectangle(4, Game.WindowHeight - 42, 164, 36), Color.White * 0.9f);
            Diamond.Animator.Draw(diamond);
            Game.SpriteBatch.DrawString(Game.Menlo, $"{Score}", new Vector2(diamond.Center.X + 50, diamond.Top + 2), Color.Black);
        }

        public void Die()
        {
            Animator.PlayState("death", () =>
            {
                Level.ParticleController.Spawn(new ParticleConstructor
                {
                    Position = Center,
                    Texture = Animator?.Anim.Textures[0],
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