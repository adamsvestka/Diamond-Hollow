using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // The second most common enemy
    // Spawns attached to walls, cannot move, but shoots projectiles at set intervals
    // On its own, it's nearly harmless, it's meant to restrict the player's movement and paired with other enemies it adds to the chaos
    // Shooting speed & health scale with difficulty
    public class WallShooter : Enemy
    {
        private enum Countdowns { Shoot, Animation }

        public static new readonly Point Size = new(48);
        public Vector2 Targeting;   // Facing direction

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