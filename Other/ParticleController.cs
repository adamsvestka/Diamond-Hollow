using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    public record struct ParticleConstructor(
        Point Position,         // Spawn location
        Color Color,            // Particle color
        Texture2D Texture,      // Particle colors are sampled from it, will take priority over color if set
        int Count,              // Particle count
        float? SpawnRadius,     // Maximum spawn distance from position
        float? DispersionSpeed, // Maximum initial velocity of each particle
        int? LifeSpan,          // Maximum lifetime of each particle, in ticks
        int? LifeSpanVariance,  // Randomization factor for lifetime, in ticks
        bool? UsePhysics        // Whether or not the particles should collide with platforms
    );

    public class ParticleController : DHGameComponent
    {
        private readonly List<ParticleInstance> _particles;

        public ParticleController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _particles = new List<ParticleInstance>();
        }

        public void Spawn(ParticleConstructor data)
        {
            var particles = new ParticleInstance(Game, this, data);
            _particles.Add(particles);
            Level.AddComponent(particles);
        }

        internal void Despawn(ParticleInstance particles)
        {
            _particles.Remove(particles);
            Level.RemoveComponent(particles);
        }
    }
}