using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // Picking this item up fires many projectiles in all directions and grants the player invulnerability for a short time (invulnerability prevents the player from taking damage & shooting)
    // Damage scales with difficulty
    // Is not attracted to the player, but has a large hitbox
    public class Nuke : Collectible
    {
        public static new readonly Point Size = new(42);

        public static Animator Animator;

        public Nuke(DiamondHollowGame game, CollectiblesController controller, Point pos) : base(game, controller, new Rectangle(pos - Size.Half(), Size))
        {
            AttractionStrength = 0f;
            OnCollect += self =>
            {
                for (int i = 0; i < 360; i += 6)    // Spawn at 6 degree intervals, meaning there will be 60 projectiles in total
                {
                    var angle = MathHelper.ToRadians(i);
                    var velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    Level.Player.Invincible = true;
                    Level.ProjectileController.Spawn(new ProjectileConstructor
                    {
                        Owner = Level.Player,
                        Origin = Center,
                        Direction = velocity,
                        Damage = (int)(30 * Level.Modifier),
                        Speed = 15,
                        Type = ProjectileType.Bullet,
                    });
                }
            };
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Animator.DrawBatch(Bounds.ToScreen());
        }
    }
}