using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    public record struct ParticleConstructor(
        Point Position,
        Color Color,
        Texture2D Texture,
        int Count,
        float? SpawnRadius,
        float? DispersionSpeed,
        int? LifeSpan,
        int? LifeSpanVariance,
        bool? UsePhysics
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