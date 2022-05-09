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

    internal class ParticleInstance : GameComponent
    {
        private List<Particle> _particles;
        private Random _random;

        public ParticleInstance(DiamondHollowGame game, ParticleConstructor data) : base(game)
        {
            _particles = new();
            _random = new();
            for (int i = 0; i < data.Count; i++)
            {
                _particles.Add(new Particle
                {
                    Color = data.Color,
                    Body = new(Game, new Rectangle((RandomVector() * (data.SpawnRadius ?? 10)).ToPoint() + data.Position, new Point(4)))
                    {
                        Velocity = RandomVector() * (data.DispersionSpeed ?? 3),
                        Gravity = 0.1f,
                        Friction = 0.1f,
                        DisableCollisions = !data.UsePhysics ?? true,
                    },
                    Life = (data.LifeSpan ?? 100) - _random.Next((data.LifeSpanVariance ?? 0) / -2, (data.LifeSpanVariance ?? 0) / 2),
                });
            }
        }

        private Vector2 RandomVector()
        {
            var angle = _random.NextDouble() * Math.PI * 2;
            var length = _random.NextDouble();
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (float)length;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _particles = _particles.Select(p =>
            {
                p.Life--;
                p.Body.Update(gameTime);
                if (Game.Level.IsWall(p.Body.Position)) p.Life = 0;
                return p;
            }).Where(p => p.Life > 0).ToList();

            if (_particles.Count == 0) Game.ParticleController.DespawnParticles(this);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();

            foreach (var particle in _particles)
            {
                Game.SpriteBatch.Draw(Game.WhitePixel, particle.Body.BoundingBox.ToScreen(), particle.Color);
            }

            Game.SpriteBatch.End();
        }
    }
}