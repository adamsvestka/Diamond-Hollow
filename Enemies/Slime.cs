using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Slime : Enemy
    {
        public static new readonly Point Size = new(48);

        public Slime(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, 20, new Rectangle(position - Size.Half(), Size))
        {
            Velocity = new Vector2(3, 0) * Level.Modifier;

            Animator = new(Game, Level, "Sprites/Slime/Idle", 10, new(19, 24, 24, 24));
            Animator.AddState("attack", new(19, 24, 24, 24), "Sprites/Slime/Ability", "Sprites/Slime/AbilityFX");
            Animator.AddState("hit", new(19, 24, 24, 24), "Sprites/Slime/Hit");
            Animator.AddState("death", new(19, 24, 24, 24), "Sprites/Slime/Death");

            Level.AddComponent(Animator);
        }

        public override void Update(GameTime gameTime)
        {
            if (Level.IsOnGround(Bounds) && !Level.IsOnGround(Bounds.OffsetX((int)(Math.Sign(Velocity.X) * Size.X * 1f)))) Velocity.X = -Velocity.X;
            var prev = Velocity;

            base.Update(gameTime);

            if (Velocity == Vector2.Zero && !Dead) Velocity = -prev;
        }

        public override void Draw(GameTime gameTime)
        {
            Animator.DrawBatch(Bounds.ToScreen(), Velocity.X < 0);

            base.Draw(gameTime);
        }
    }
}