using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public enum CameraState
    {
        Tracking,
        Scrolling,
    }

    public delegate void OnScrollComplete();

    record struct CameraScrollingState(float From, float To, float Duration, OnScrollComplete callback)
    {
        public float Elapsed { get; set; } = default;
    }

    public class Camera : DHGameComponent
    {

        public int CameraY { get; private set; }
        public int VelocityY { get; private set; }

        private CameraState _state;
        private CollisionBody _trackingBody;
        private CameraScrollingState _scrollingState;


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
                    int screenHeight = Game.Height;
                    int delta = _trackingBody.Center.Y + (int)_trackingBody.Velocity.Y - CameraY - screenHeight / 2;

                    if (Math.Abs(VelocityY) < 2.5) VelocityY = 0;
                    VelocityY -= (int)(Math.Sign(VelocityY) * 2.5);
                    if (delta > screenHeight / 8) VelocityY = Math.Max((int)Math.Pow((delta - screenHeight / 8) / 25, 2), VelocityY);
                    else if (delta < -screenHeight / 4) VelocityY = Math.Min(-(int)Math.Pow((delta + screenHeight / 4) / -25, 2), VelocityY);
                    CameraY += VelocityY;

                    int levelHeight = Level.GetHeight() - Game.Height;
                    if (CameraY < 0) CameraY = 0;
                    else if (CameraY > levelHeight) CameraY = levelHeight;

                    break;

                case CameraState.Scrolling:
                    if (_scrollingState.Elapsed == _scrollingState.Duration)
                    {
                        _state = CameraState.Tracking;
                        _scrollingState.callback?.Invoke();
                        break;
                    }

                    float t = 0.5f - (float)Math.Cos(_scrollingState.Elapsed++ / _scrollingState.Duration * Math.PI) / 2f;
                    CameraY = (int)(_scrollingState.From + (_scrollingState.To - _scrollingState.From) * t);

                    break;
            }
        }

        public void Track(CollisionBody body)
        {
            _state = CameraState.Tracking;
            _trackingBody = body;
        }

        public void Scroll(int posY, int duration, OnScrollComplete callback)
        {
            _state = CameraState.Scrolling;
            _scrollingState = new CameraScrollingState(CameraY, Math.Max(posY - Game.Height / 2, 0), duration, callback);
            VelocityY = 0;
        }
    }
}