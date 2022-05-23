using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    public class Enemy : PhysicsBody
    {
        public EnemyController Controller;
        public int MaxHealth { get; init; }
        public int Health { get; private set; }

        protected float HeartDropChance = 0.075f;
        protected int DiamondDropCount = 10;

        public Animator Animator;
        public bool Dead => Health <= 0;

        private Texture2D _healthbarFullTexture;

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
                    if (Animator?.HasState("death") == true)
                    {
                        Velocity = new Vector2(Math.Sign(Velocity.X), Math.Sign(Velocity.Y)) / 1000f;
                        Gravity = 0f;
                        DisableCollisionBox = true;
                        Animator.PlayState("death", Die);
                    }
                    else Die();
                }
                else if (Animator?.HasState("hit") == true) Animator.PlayState("hit");
            };
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _healthbarFullTexture = Game.Content.Load<Texture2D>("Sprites/UI/Healthbar");
        }

        private void Die()
        {
            Controller.Despawn(this);
            Level.CollectiblesController.SpawnDiamondCluster(Center, (int)(DiamondDropCount * Level.Modifier), 15);
            if (Game.Chance(HeartDropChance * Level.Modifier)) Level.CollectiblesController.SpawnSmallHeart(Center);
            Level.ParticleController.Spawn(new ParticleConstructor
            {
                Position = Center,
                Color = Color.Red,
                Texture = Animator?.Anim.Textures[0],
                Count = 100,
                DispersionSpeed = 1.5f,
                SpawnRadius = Size.X / 2,
                LifeSpan = 40,
                LifeSpanVariance = 15,
                UsePhysics = true,
            });
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Bounds.Intersects(Level.Player.Bounds) && !DisableCollisionBox && !Level.Player.Invincible)
            {
                if (Animator?.HasState("attack") == true) Animator.PlayState("attack");
                Level.Player.OnEnemyCollision(this);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Health == MaxHealth || Dead) return;

            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            int width = (int)(60 * Level.Modifier);
            var bar = new Rectangle(Center.X - width / 2, Center.Y + Size.Y / 2 + 10, width, 10);
            Level.DrawRectangle(bar.Grow(2), Color.Black);
            Level.DrawRectangle(bar, Color.White);
            bar.Width = (int)(width * (Health / (float)MaxHealth));
            var cutout = new Rectangle(40, 85, (int)Math.Clamp(bar.Width, 0f, 66f), 5);
            Game.SpriteBatch.Draw(_healthbarFullTexture, bar.ToScreen(), cutout, Color.White);

            Game.SpriteBatch.End();
        }
    }
}