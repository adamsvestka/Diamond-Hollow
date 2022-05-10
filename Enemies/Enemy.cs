using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Enemy : PhysicsBody
    {
        public EnemyController Controller;

        public Enemy(DiamondHollowGame game, EnemyController controller, Rectangle bounds) : base(game, controller.Level, bounds)
        {
            Controller = controller;
            Friction = 0;
            OnProjectileHit += proj =>
            {
                Controller.Despawn(this);
                Level.ParticleController.Spawn(new ParticleConstructor
                {
                    Position = Center,
                    Color = Color.Red,
                    Count = 100,
                    DispersionSpeed = 1.5f,
                    SpawnRadius = 10,
                    LifeSpan = 40,
                    LifeSpanVariance = 15,
                    UsePhysics = true,
                });
                Level.DiamondController.SpawnCluster(Center, 5, 10);
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Bounds.Intersects(Level.Player.Bounds)) Level.Player.OnEnemyCollision(this);
        }
    }
}