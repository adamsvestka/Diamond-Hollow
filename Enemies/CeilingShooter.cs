using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // A less common enemy, is meant to prevent the player from standing still
    // Spawns attached to ceilings, cannot move, but shoots projectiles at set intervals
    // It will shoot a projectile downward aiming at the player
    // Shooting speed & health scale with difficulty
    public class CeilingShooter : Enemy
    {
        private enum Countdowns { Shoot, Animation }

        public static new readonly Point Size = new(50);
        public Vector2 Targeting;

        public CeilingShooter(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, 30, new Rectangle(position - Size.Half(), Size))
        {
            Targeting = Vector2.Zero;
            Gravity = 0;

            CreateCountdown((int)Countdowns.Animation, (int)(250 / Level.Modifier), true, 40, () => Animator.PlayState("shot"));
            CreateCountdown((int)Countdowns.Shoot, (int)(250 / Level.Modifier), true, 0, () =>
            {
                if (Targeting.Y > 0 || Dead) return;
                Level.ProjectileController.Spawn(new ProjectileConstructor
                {
                    Owner = this,
                    Origin = Center,
                    Direction = Targeting,
                    Size = new Point(24),
                    Speed = 5 * Level.Modifier,
                    Type = ProjectileType.Fireball,
                });
            });

            Animator = new Animator(Game, Level, "Sprites/CeilingShooter/Idle", 10);

            Animator.AddState("shot", Rectangle.Empty, "Sprites/CeilingShooter/Shoot");
            Animator.AddState("hit", Rectangle.Empty, "Sprites/CeilingShooter/Hit");
            Animator.AddState("death", Rectangle.Empty, "Sprites/CeilingShooter/Death");

            Level.AddComponent(Animator);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Targeting = (Level.Player.Center - Center).ToVector2();
            Targeting.Normalize();
        }

        public override void Draw(GameTime gameTime)
        {
            Animator.DrawBatch(Bounds.OffsetY(5).ToScreen());

            base.Draw(gameTime);
        }
    }
}