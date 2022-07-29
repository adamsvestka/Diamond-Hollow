using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// A small single-colored rectangle.
    /// </summary>
    internal struct Particle
    {
        /// <summary>A body to simulate physics for the particle.</summary>
        public PhysicsBody Body;
        /// <summary>The color of the particle.</summary>
        public Color Color;
        /// <summary>The amount of time the particle will live.</summary>
        public int Life { get; set; } = default;

        /// <summary>
        /// Creates a new particle.
        /// </summary>
        /// <param name="body">The body to simulate physics for the particle.</param>
        /// <param name="color">The color of the particle.</param>
        public Particle(PhysicsBody body, Color color)
        {
            Body = body;
            Color = color;
        }
    }

    /// <summary>
    /// Will appear as a collection of small colored rectangles dispersed around a point.
    /// They will move around and disappear after a certain amount of time.
    /// They may or may not collide with platforms.
    /// </summary>
    internal class ParticleInstance : DHGameComponent
    {
        /// <summary>
        /// A controller for the particles.
        /// </summary>
        private readonly ParticleController Controller;
        /// <summary>
        /// A list of particles.
        /// </summary>
        private List<Particle> _particles;
        /// <summary>
        /// A random number generator for particle positions and life spans.
        /// </summary>
        private readonly Random Random;

        /// <summary>
        /// Creates a new particle instance.
        /// 
        /// The particles' colors will be randomly selected from the provided texture. If not specified, the color will be a single color.
        /// 
        /// The particles will have randomized positions, velocities, and lifetimes. See <see cref="DiamondHollow.ParticleConstructor"/> for more information.
        /// 
        /// Do not use this constructor directly. Use <see cref="DiamondHollow.ParticleController.Spawn"/> instead.
        /// </summary>
        /// <param name="game">The game the particle instance belongs to.</param>
        /// <param name="controller">The controller for the particles.</param>
        /// <param name="data">The data for the particle instance.</param>
        /// <returns>The new particle instance.</returns>
        public ParticleInstance(DiamondHollowGame game, ParticleController controller, ParticleConstructor data) : base(game, controller.Level)
        {
            Controller = controller;
            DrawOrder = (int)DrawingLayers.Particles;
            _particles = new();
            Random = new();

            // Create a color palette for the particles, either from a texture or from a single color
            Color[] colorData;
            if (data.Texture != null)
            {
                colorData = new Color[data.Texture.Width * data.Texture.Height];
                data.Texture.GetData(colorData);
            }
            else colorData = new Color[1] { data.Color };
            // Select a random color without alpha from the palette
            var sampleColor = () =>
            {
                Color c;
                do c = colorData[Random.Next(colorData.Length) % colorData.Length];
                while (c.A == 0);
                return c;
            };

            // Create the particles, with randomized positions, velocities and lifetimes
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

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Updates the particle positions and lifetimes.
        /// Also despawn particles that have exceeded their lifetime or got stuck in a wall.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Records are immutable, so we need to create a new list to update the particles
            // Also despawn particles that have exceeded their lifetime or got stuck in a wall
            _particles = _particles.Select(particle =>
            {
                particle.Life--;
                particle.Body.Update(gameTime);
                if (Level.IsWall(particle.Body.Position)) particle.Life = 0;
                return particle;
            }).Where(particle => particle.Life > 0).ToList();

            if (_particles.Count == 0) Controller.Despawn(this);
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draws the particles.
        /// </summary>
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