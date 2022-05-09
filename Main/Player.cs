using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    public class Player : PhysicsBody
    {
        public Vector2 Targeting;
        public List<CollisionBody> Projectiles;

        public Player(DiamondHollowGame game) : base(game, new Rectangle(new Point(125, 125), new Point(48)))
        {
            Projectiles = new();
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.KeyboardState.IsKeyDown(Keys.Space) && IsOnGround) Velocity.Y = 21;
            if (Game.KeyboardState.IsKeyDown(Keys.A)) Velocity.X = -6;
            if (Game.KeyboardState.IsKeyDown(Keys.D)) Velocity.X = 6;
            if (Game.ButtonPressed(MouseButton.Right)) Game.ParticleController.SpawnParticles(new ParticleConstructor
            {
                Position = Center,
                Color = Color.Green,
                Count = 100
            });
            if (Game.ButtonPressed(MouseButton.Left)) Shoot();

            base.Update(gameTime);

            Targeting = (Mouse.GetState().Position.ToScreen() - Center).ToVector2();
            Targeting.Normalize();
        }

        private void Shoot()
        {
            Vector2 size = new(8);
            var proj = new CollisionBody(Game, new Rectangle(Center - (size / 2).ToPoint(), size.ToPoint()))
            {
                Velocity = Targeting * 10,
            };
            proj.OnCollision += point =>
            {
                Projectiles.Remove(proj);
                Game.Components.Remove(proj);
                Game.ParticleController.SpawnParticles(new ParticleConstructor
                {
                    Position = point,
                    Color = Color.Red,
                    Count = 50,
                    DispersionSpeed = 1.5f,
                    SpawnRadius = 0,
                    LifeSpan = 40,
                    LifeSpanVariance = 15,
                    UsePhysics = true,
                });
            };
            Projectiles.Add(proj);
            Game.Components.Add(proj);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();

            DrawCrosshairs();
            Game.SpriteBatch.Draw(Game.WhitePixel, BoundingBox.ToScreen(), Color.Blue);
            Projectiles.ForEach(p => Game.SpriteBatch.Draw(Game.WhitePixel, p.BoundingBox.ToScreen(), Color.Purple));

            // DrawDebug();

            Game.SpriteBatch.End();
        }

        private void DrawCrosshairs()
        {
            var center = BoundingBox.Center.ToVector2();
            var size = new Vector2(4);
            var spacing = 20;
            var count = (GraphicsDevice.Viewport.Width + GraphicsDevice.Viewport.Height) / spacing;
            for (int i = 0; i < count; i++)
            {
                var dot = center + Targeting * i * spacing;
                Game.SpriteBatch.Draw(Game.WhitePixel, new Rectangle((dot - size / 2).ToPoint(), size.ToPoint()).ToScreen(), Color.Red);
            }
        }

        private void DrawDebug()
        {
            var box = BoundingBox;
            var box2 = new Rectangle(Position + Velocity.ToPoint(), Size);

            foreach (var point in box2.Corners())
                Game.SpriteBatch.Draw(Game.WhitePixel, point.SnapToGrid().MakeTile().ToScreen(), Color.BlueViolet);

            foreach (var point in box.Corners())
                Game.SpriteBatch.Draw(Game.WhitePixel, point.SnapToGrid().MakeTile().ToScreen(), Color.Blue);

            Game.SpriteBatch.Draw(Game.WhitePixel, box2.ToScreen(), Color.Yellow);
            Game.SpriteBatch.Draw(Game.WhitePixel, box.ToScreen(), Game.Level.IsOnGround(box) ? Color.Green : Color.Red);

            Game.SpriteBatch.DrawString(Game.Menlo, String.Join("\n", BoundingBox.Corners().Select(p => p.ToGrid())), new Vector2(10, 10), Color.Black);
        }
    }
}