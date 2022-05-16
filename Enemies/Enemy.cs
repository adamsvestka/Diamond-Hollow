using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Enemy : PhysicsBody
    {
        public EnemyController Controller;
        public int MaxHealth { get; init; }
        public int Health { get; private set; }

        public Enemy(DiamondHollowGame game, EnemyController controller, int maxHealth, Rectangle bounds) : base(game, controller.Level, bounds)
        {
            Controller = controller;
            Health = MaxHealth = (int)(maxHealth * Level.Modifier / 5f) * 10;
            Friction = 0;
            OnProjectileHit += proj =>
            {
                if (proj.Owner != Level.Player) return;
                if ((Health -= proj.Damage) <= 0)
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
                    Level.CollectiblesController.SpawnDiamondCluster(Center, (int)(10 * Level.Modifier), 15);
                    if (Game.Chance(0.05f)) Level.CollectiblesController.SpawnSmallHeart(Center);
                }
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Bounds.Intersects(Level.Player.Bounds)) Level.Player.OnEnemyCollision(this);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Health == MaxHealth) return;

            Game.SpriteBatch.Begin();

            int width = 40;
            var bar = new Rectangle(Center.X - width / 2, Center.Y + Size.Y / 2 + 10, width, 10);
            Game.Level.DrawRectangle(bar, Color.Gray);
            bar.Width = (int)(width * (Health / (float)MaxHealth));
            Game.Level.DrawRectangle(bar, Color.Red);

            Game.SpriteBatch.End();
        }
    }
}