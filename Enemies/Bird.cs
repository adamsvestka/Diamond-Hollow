using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Bird : Enemy
    {
        private enum Countdowns { Shoot }

        public static new readonly Point Size = new(68, 48);
        public Vector2 Targeting;

        public Bird(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, 40, new Rectangle(position - Size.Half(), Size))
        {
            Velocity = new Vector2(4.5f, 0) * Level.Modifier;
            Gravity = 0;
            Targeting = new Vector2(0, -1);
            HeartDropChance = 0.7f;
            DiamondDropCount = 10;

            CreateCountdown((int)Countdowns.Shoot, (int)(150 / Level.Modifier), false);

            Animator = new Animator(Game, Level, "Sprites/Bird/Idle", 10, new(4, 4, 34, 34));

            Animator.AddState("death", new(4, 4, 24, 24), "Sprites/Bird/Death");

            Level.AddComponent(Animator);
        }

        public override void Update(GameTime gameTime)
        {
            var prev = Velocity;

            base.Update(gameTime);

            if (Velocity == Vector2.Zero) Velocity = -prev;

            if (Level.Difficulty > 5 && IsCountdownDone((int)Countdowns.Shoot) && CheckForPlayer())
            {
                Level.ProjectileController.Spawn(new ProjectileConstructor
                {
                    Owner = this,
                    Origin = Center,
                    Direction = Targeting,
                    Size = new Point(32),
                    Speed = 6.5f * Level.Modifier,
                    Type = ProjectileType.Fireball,
                });
                ResetCountdown((int)Countdowns.Shoot);
            }
        }

        private bool CheckForPlayer()
        {
            int xdiff = Center.X - Level.Player.Center.X;
            int ydiff = Center.Y - Level.Player.Center.Y;
            return Math.Abs(xdiff) < Level.Player.Size.X / 2 && 0 < ydiff && ydiff < 8 * Game.TileSize;
        }

        public override void Draw(GameTime gameTime)
        {
            Animator.DrawBatch(new Rectangle(Center - new Point(Size.X).Half(), new Point(Size.X)).ToScreen(), Velocity.X < 0);

            base.Draw(gameTime);
        }
    }
}