using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    public class Player : PhysicsBody
    {
        public Vector2 Targeting;
        private int _invincibleDuration;
        public bool Invincible => _invincibleDuration > 0;

        public Player(DiamondHollowGame game, Level level) : base(game, level, new Rectangle(new Point(125, 125), new Point(36))) { }

        public override void Initialize()
        {
            base.Initialize();

            Targeting = Vector2.Zero;
            _invincibleDuration = 0;

            OnProjectileHit += OnEnemyCollision;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Locked)
            {
                if (Game.KeyboardState.IsKeyDown(Keys.Space) && IsOnGround) Velocity.Y = 21;
                if (Game.KeyboardState.IsKeyDown(Keys.A)) Velocity.X = -6;
                if (Game.KeyboardState.IsKeyDown(Keys.D)) Velocity.X = 6;
                if (Game.ButtonPressed(MouseButton.Left))
                    Game.Level.ProjectileController.Spawn(new ProjectileConstructor
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

            DrawCrosshairs();
            Game.Level.DrawRectangle(Bounds, Color.Blue * (Invincible ? 0.5f : 1f));

            Game.SpriteBatch.End();
        }

        private void DrawCrosshairs()
        {
            var center = Bounds.Center.ToVector2();
            var size = new Vector2(4);
            var spacing = 20;
            var count = (GraphicsDevice.Viewport.Width + GraphicsDevice.Viewport.Height) / spacing;
            for (int i = 0; i < count; i++)
            {
                var dot = center + Targeting * i * spacing;
                Game.Level.DrawRectangle(new Rectangle((dot - size / 2).ToPoint(), size.ToPoint()), Color.Red);
            }
        }

        public void OnEnemyCollision(CollisionBody enemy)
        {
            if (Invincible) return;
            _invincibleDuration = 90;
            var dir = (Center - enemy.Center).ToVector2();
            dir.X = Math.Sign(dir.X);
            dir.Y = dir.Y == 0 ? 1 : Math.Sign(dir.Y);
            Yeet(dir * 6);
        }
    }
}