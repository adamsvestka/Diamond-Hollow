using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    public class Player : PhysicsBody
    {
        public Vector2 Targeting;
        private int _invincibleDuration;
        public bool Invincible => _invincibleDuration > 0;
        public int MaxHearts { get; private set; }
        public int Hearts { get; private set; }
        public int Score { get; private set; }

        public Player(DiamondHollowGame game, Level level) : base(game, level, new Rectangle(new Point(125, 125), new Point(36))) { }

        public override void Initialize()
        {
            base.Initialize();

            Targeting = Vector2.Zero;
            _invincibleDuration = 0;
            MaxHearts = Hearts = 3;

            OnProjectileHit += OnEnemyCollision;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Locked && Hearts > 0)
            {
                if (Game.KeyboardState.IsKeyDown(Keys.Space) && IsOnGround) Velocity.Y = 21;
                if (Game.KeyboardState.IsKeyDown(Keys.A)) Velocity.X = -6;
                if (Game.KeyboardState.IsKeyDown(Keys.D)) Velocity.X = 6;
                if (Game.ButtonPressed(MouseButton.Left) && !Invincible)
                    Level.ProjectileController.Spawn(new ProjectileConstructor
                    {
                        Owner = this,
                        Origin = Center + Targeting.ToPoint(),
                        Direction = Targeting,
                        Color = Color.Purple,
                        Speed = 15,
                    });
            }

            if (Invincible) _invincibleDuration--;

            base.Update(gameTime);

            Targeting = (Mouse.GetState().Position.ToScreen() - Center).ToVector2();
            Targeting.Normalize();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();

            if (Hearts > 0) DrawCrosshairs();
            Level.DrawRectangle(Bounds, Color.Blue * (Invincible ? 0.5f : 1f));
            DrawHealthbar();
            DrawScore();

            Game.SpriteBatch.End();
        }

        private void DrawCrosshairs()
        {
            var center = Bounds.Center.ToVector2();
            var size = new Vector2(4);
            var spacing = 20;
            var count = (Game.Width + Game.Height) / spacing;
            for (int i = 0; i < count; i++)
            {
                var dot = center + Targeting * i * spacing;
                Level.DrawRectangle(new Rectangle((dot - size / 2).ToPoint(), size.ToPoint()), Color.Red);
            }
        }

        private void DrawHealthbar()
        {
            var heart = new Rectangle(13, 13, 23, 23);
            for (int i = 0; i < MaxHearts; i++)
            {
                Game.SpriteBatch.Draw(Game.WhitePixel, heart, i >= Hearts ? Color.Gray : Color.Red);
                heart.X += heart.Width + 7;
            }
        }

        private void DrawScore()
        {
            var diamond = new Rectangle(new Point(13, Game.Height - Diamond.Size.Y - 13), Diamond.Size);
            Game.SpriteBatch.Draw(Game.WhitePixel, diamond, Color.Green);
            Game.SpriteBatch.DrawString(Game.Menlo, $"{Score}", new Vector2(diamond.Center.X + 50, diamond.Top), Color.Green);
        }

        public void Respawn()
        {
            Position = Level.Spawnpoint - Size.Half();
            Velocity = Vector2.Zero;
            Level.Camera.Scroll(Center.Y, 120, () => Hearts = MaxHearts);
        }

        public void OnEnemyCollision(CollisionBody enemy)
        {
            if (Invincible) return;
            if (--Hearts == 0) { Respawn(); return; }
            _invincibleDuration = 90;
            var dir = (Center - enemy.Center).ToVector2();
            dir.X = Math.Sign(dir.X);
            dir.Y = dir.Y == 0 ? 1 : Math.Sign(dir.Y);
            Yeet(dir * 6);
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