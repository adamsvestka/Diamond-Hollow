using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    internal record struct Particle(PhysicsBody Body, Color Color)
    {
        public int Life { get; set; } = default;
    }

    internal class ParticleInstance : DHGameComponent
    {
        private readonly ParticleController Controller;
        private List<Particle> _particles;
        private readonly Random Random;

        public ParticleInstance(DiamondHollowGame game, ParticleController controller, ParticleConstructor data) : base(game, controller.Level)
        {
            Controller = controller;
            DrawOrder = (int)DrawingLayers.Particles;
            _particles = new();
            Random = new();

            Color[] colorData;
            if (data.Texture != null)
            {
                colorData = new Color[data.Texture.Width * data.Texture.Height];
                data.Texture.GetData(colorData);
            }
            else colorData = new Color[1] { data.Color };
            var sampleColor = () =>
            {
                Color c;
                do c = colorData[Random.Next(colorData.Length) % colorData.Length];
                while (c.A == 0);
                return c;
            };

            for (int i = 0; i < data.Count; i++)
            {
                _particles.Add(new Particle
                {
                    Color = sampleColor(),
                    Body = new(Game, Level, new Rectangle(data.Position.RandomOffset(data.SpawnRadius ?? 10) - new Point(2), new Point(4)))
                    {
                        Velocity = Vector2.Zero.RandomOffset(data.DispersionSpeed ?? 3),
                        Gravity = 0.1f,
                        Friction = 0.1f,
                        DisableCollisions = !data.UsePhysics ?? true,
                        DisableCollisionBox = true,
                    },
                    Life = (data.LifeSpan ?? 100) - Random.Next((data.LifeSpanVariance ?? 0) / -2, (data.LifeSpanVariance ?? 0) / 2),
                });
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _particles = _particles.Select(particle =>
            {
                particle.Life--;
                particle.Body.Update(gameTime);
                if (Level.IsWall(particle.Body.Position)) particle.Life = 0;
                return particle;
            }).Where(particle => particle.Life > 0).ToList();

            if (_particles.Count == 0) Controller.Despawn(this);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();

            foreach (var particle in _particles)
            {
                Level.DrawRectangle(particle.Body.Bounds, particle.Color);
            }

            Game.SpriteBatch.End();
        }
    }
}