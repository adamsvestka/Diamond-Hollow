using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    /// <summary>
    /// ParticleConstructor is used to specify the properties of a particle instance.
    /// </summary>
    public struct ParticleConstructor
    {
        /// <summary>Spawn location.</summary>
        public Point Position;
        /// <summary>Particle color.</summary>
        public Color Color;
        /// <summary>Particle colors are sampled from it, will take priority over color if set.</summary>
        public Texture2D Texture;
        /// <summary>Particle count.</summary>
        public int Count;
        /// <summary>Maximum spawn distance from position.</summary>
        public float? SpawnRadius;
        /// <summary>Maximum initial velocity of each particle.</summary>
        public float? DispersionSpeed;
        /// <summary>Maximum lifetime of each particle, in ticks.</summary>
        public int? LifeSpan;
        /// <summary>Randomization factor for lifetime, in ticks.</summary>
        public int? LifeSpanVariance;
        /// <summary>Whether or not the particles should collide with platforms.</summary>
        public bool? UsePhysics;

        /// <summary>
        /// Creates a new particle constructor.
        /// </summary>
        /// <param name="position">Spawn location.</param>
        /// <param name="color">Particle color.</param>
        /// <param name="texture">Particle colors are sampled from it, will take priority over color if set.</param>
        /// <param name="count">Particle count.</param>
        /// <param name="spawnRadius">Maximum spawn distance from position.</param>
        /// <param name="dispersionSpeed">Maximum initial velocity of each particle.</param>
        /// <param name="lifeSpan">Maximum lifetime of each particle, in ticks.</param>
        /// <param name="lifeSpanVariance">Randomization factor for lifetime, in ticks.</param>
        /// <param name="usePhysics">Whether or not the particles should collide with platforms.</param>
        public ParticleConstructor(Point position, Color color, Texture2D texture, int count, float? spawnRadius, float? dispersionSpeed, int? lifeSpan, int? lifeSpanVariance, bool? usePhysics)
        {
            Position = position;
            Color = color;
            Texture = texture;
            Count = count;
            SpawnRadius = spawnRadius;
            DispersionSpeed = dispersionSpeed;
            LifeSpan = lifeSpan;
            LifeSpanVariance = lifeSpanVariance;
            UsePhysics = usePhysics;
        }
    }

    /// <summary>
    /// A class to handle spawning and despawning particle instances.
    /// </summary>
    public class ParticleController : DHGameComponent
    {
        /// <summary>
        /// A list of particle instances.
        /// </summary>
        private readonly List<ParticleInstance> _particles;

        /// <summary>
        /// Creates a new particle controller.
        /// </summary>
        /// <param name="game">The game the particle controller belongs to.</param>
        /// <param name="level">The level the particle controller belongs to.</param>
        /// <returns>The new particle controller.</returns>
        public ParticleController(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
            _particles = new List<ParticleInstance>();
        }

        /// <summary>
        /// Spawns particles.
        /// </summary>
        /// <param name="data">The data to spawn particles with.</param>
        /// <seealso cref="DiamondHollow.ParticleInstance"/>
        public void Spawn(ParticleConstructor data)
        {
            var particles = new ParticleInstance(Game, this, data);
            _particles.Add(particles);
            Level.AddComponent(particles);
        }

        /// <summary>
        /// Despawns particles.
        /// </summary>
        /// <param name="particles">The particle instance to despawn.</param>
        internal void Despawn(ParticleInstance particles)
        {
            _particles.Remove(particles);
            Level.RemoveComponent(particles);
        }
    }
}