using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class Camera : DHGameComponent
    {
        private enum CameraState { Tracking, Scrolling }
        private enum Countdowns { Scroll }

        public int CameraY { get; private set; }
        public int VelocityY { get; private set; }

        private CameraState _state;
        private CollisionBody _trackingBody;
        private (int From, int To) _scrollingState;

        public Camera(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            switch (_state)
            {
                case CameraState.Tracking:
                    int screenHeight = Game.WindowHeight;
                    int delta = _trackingBody.Center.Y + (int)_trackingBody.Velocity.Y - CameraY - screenHeight / 2;

                    if (Math.Abs(VelocityY) < 2.5) VelocityY = 0;
                    VelocityY -= (int)(Math.Sign(VelocityY) * 2.5);
                    if (delta > screenHeight / 8) VelocityY = Math.Max((int)Math.Pow((delta - screenHeight / 8) / 25, 2), VelocityY);
                    else if (delta < -screenHeight / 4) VelocityY = Math.Min(-(int)Math.Pow((delta + screenHeight / 4) / -25, 2), VelocityY);
                    CameraY += VelocityY;

                    int levelHeight = Level.MapHeight - Game.WindowHeight;
                    if (CameraY < 0) CameraY = 0;
                    else if (CameraY > levelHeight) CameraY = levelHeight;

                    break;

                case CameraState.Scrolling when IsCountdownDone((int)Countdowns.Scroll):
                    _state = CameraState.Tracking;
                    break;

                case CameraState.Scrolling:
                    float t = 0.5f - (float)Math.Cos(GetCountdownProgress((int)Countdowns.Scroll) * Math.PI) / 2f;
                    CameraY = (int)(_scrollingState.From + (_scrollingState.To - _scrollingState.From) * t);
                    break;
            }
        }

        public void Track(CollisionBody body)
        {
            _state = CameraState.Tracking;
            _trackingBody = body;
        }

        public void Scroll(int posY, int duration, Action callback)
        {
            _state = CameraState.Scrolling;
            VelocityY = 0;
            _scrollingState = (CameraY, Math.Max(posY - Game.WindowHeight / 2, 0));
            CreateCountdown((int)Countdowns.Scroll, duration, false, 0, callback);
        }
    }
}