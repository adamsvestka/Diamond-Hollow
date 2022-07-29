using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// Picking this item up fires many projectiles in all directions and grants the player invulnerability for a short time (invulnerability prevents the player from taking damage &amp; shooting).
    /// Damage scales with difficulty.
    /// It is not attracted to the player, but has a large hitbox.
    /// </summary>
    public class Nuke : Collectible
    {
        /// <summary>
        /// The size of a nuke.
        /// </summary>
        public static new readonly Point Size = new(42);

        /// <summary>
        /// The nuke's animation handler.
        /// </summary>
        public static Animator Animator;

        /// <summary>
        /// Creates a new nuke at the given position.
        /// 
        /// Do not use this constructor directly. Instead, use the <see cref="DiamondHollow.CollectiblesController.SpawnNuke"/> method.
        /// </summary>
        /// <param name="game">The game that this component is a part of.</param>
        /// <param name="controller">The level that this component is a part of.</param>
        /// <param name="pos">The position of the nuke.</param>
        /// <returns>The new nuke.</returns>
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

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draws the animating nuke.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Animator.DrawBatch(Bounds.ToScreen());
        }
    }
}