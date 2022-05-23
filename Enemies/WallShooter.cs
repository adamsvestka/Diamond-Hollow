using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class WallShooter : Enemy
    {
        private enum Countdowns { Shoot, Animation }

        public static new readonly Point Size = new(48);
        public Vector2 Targeting;

        public WallShooter(DiamondHollowGame game, EnemyController controller, Point position, bool facingRight) : base(game, controller, 20, new Rectangle(position - Size.Half(), Size))
        {
            Targeting = new Vector2(facingRight ? 1 : -1, 0);
            Gravity = 0;

            CreateCountdown((int)Countdowns.Animation, (int)(250 / Level.Modifier), true, 120, () => Animator.PlayState("shot"));
            CreateCountdown((int)Countdowns.Shoot, (int)(250 / Level.Modifier), true, 100, () =>
            {
                if (Dead) return;
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

            Animator = new Animator(Game, Level, "Sprites/WallShooter/Idle", 10);

            Animator.AddState("shot", Rectangle.Empty, "Sprites/WallShooter/Shoot");
            Animator.AddState("hit", Rectangle.Empty, "Sprites/WallShooter/Hit");
            Animator.AddState("death", Rectangle.Empty, "Sprites/WallShooter/Death");

            Level.AddComponent(Animator);
        }

        public override void Draw(GameTime gameTime)
        {
            Animator.DrawBatch(Bounds.OffsetX(Math.Sign(Targeting.X) * -5).ToScreen(), Targeting.X < 0);

            base.Draw(gameTime);
        }
    }
}