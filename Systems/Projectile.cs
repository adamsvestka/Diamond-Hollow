using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Projectile : CollisionBody
    {
        private readonly ProjectileController Controller;
        private static readonly Point _defaultSize = new(8);

        private readonly Color _color;
        public readonly CollisionBody Owner;

        public Projectile(DiamondHollowGame game, ProjectileController controller, ProjectileConstructor data)
            : base(game, controller.Level, new Rectangle(data.Origin - (data.Size ?? _defaultSize).Half(), data.Size ?? _defaultSize))
        {
            Controller = controller;
            Owner = data.Owner;
            _color = data.Color ?? Color.Red;
            DisableCollisionBox = true;
            Velocity = data.Direction * (data.Speed ?? 10);
            OnCollision += point => Controller.Despawn(this);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();
            Game.Level.DrawRectangle(Bounds, _color);
            Game.SpriteBatch.End();
        }
    }
}