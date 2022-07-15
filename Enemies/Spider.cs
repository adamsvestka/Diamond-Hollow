using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    // A rare enemy, harmless if you take note of it
    // Spawns attached to ceilings, cannot move, but will drop on top of the player if they stand under it
    // Is meant to catch the player off guard, it's often positioned above key positions in the level
    // The spider will wait on the ceiling until a player walks under it, then it drops at gravity's speed, pauses for a while and then slowly rewinds again
    // Pause speed & health scale with difficulty
    public class Spider : Enemy
    {
        private enum SpiderState { Armed, Falling, Paused, Rewinding }
        private enum Countdowns { Pause }

        public static new readonly Point Size = new(48);
        private SpiderState _state;

        public Point Origin;
        public bool IsOnCeiling => Center == Origin;

        public Spider(DiamondHollowGame game, EnemyController controller, Point position) : base(game, controller, 20, new Rectangle(position - Size.Half(), Size))
        {
            Origin = Center;
            _state = SpiderState.Armed;
            Gravity = 0;

            CreateCountdown((int)Countdowns.Pause, (int)(60 / Level.Modifier), false);

            Animator = new Animator(game, Level, "Sprites/Spider/Idle", 10);

            Animator.AddState("move", Rectangle.Empty, "Sprites/Spider/Move");
            Animator.AddState("hit", Rectangle.Empty, "Sprites/Spider/Move");
            Animator.AddState("death", Rectangle.Empty, "Sprites/Spider/Death");

            Level.AddComponent(Animator);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Dead) return;
            if (_state is SpiderState.Paused or SpiderState.Rewinding && Animator.State == "default") Animator.PlayState("move");

            switch (_state)
            {
                case SpiderState.Armed when CheckForPlayer() && IsCountdownDone((int)Countdowns.Pause):
                    _state = SpiderState.Falling;
                    Gravity = 1f;
                    break;
                case SpiderState.Falling when IsOnGround:
                    _state = SpiderState.Paused;
                    ResetCountdown((int)Countdowns.Pause);
                    break;
                case SpiderState.Paused when IsCountdownDone((int)Countdowns.Pause):
                    _state = SpiderState.Rewinding;
                    Velocity = new Vector2(0, 3);
                    Gravity = 0;
                    break;
                case SpiderState.Rewinding when IsOnCeiling:
                    _state = SpiderState.Armed;
                    Velocity = Vector2.Zero;
                    ResetCountdown((int)Countdowns.Pause);
                    break;
            }
        }

        private bool CheckForPlayer()
        {
            int xdiff = Center.X - Level.Player.Center.X;
            int ydiff = Center.Y - Level.Player.Center.Y;
            return Math.Abs(xdiff) < Size.X / 2 + Level.Player.Size.X / 2 && 0 < ydiff && ydiff < 8 * Game.TileSize;
        }

        public override void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (!Dead) Level.DrawLine(Origin.OffsetY(Size.Y), Center, Color.White, 4);  // Draws a string on which the spider is hanging
            Animator.Draw(Bounds.ToScreen());
            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}