using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public record struct ParticleConstructor(
        Point Position,
        Color Color,
        int Count,
        float? SpawnRadius,
        float? DispersionSpeed,
        int? LifeSpan,
        int? LifeSpanVariance,
        bool? UsePhysics
    );

    public class ParticleController : GameComponent
    {
        private readonly List<ParticleInstance> _particles;

        public ParticleController(DiamondHollowGame game) : base(game)
        {
            _particles = new List<ParticleInstance>();
        }

        public void SpawnParticles(ParticleConstructor data)
        {
            var particles = new ParticleInstance(Game, data);
            _particles.Add(particles);
            Game.Components.Add(particles);
        }

        internal void DespawnParticles(ParticleInstance particles)
        {
            _particles.Remove(particles);
            Game.Components.Remove(particles);
        }
    }
}