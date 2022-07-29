using System;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// This class handles smooth camera motion.
    /// 
    /// It has two modes:
    /// - Tracking a target: The camera will keep the target in frame.
    /// - Scroll to a location: The camera will move to a location over a period of time.
    /// </summary>
    public class Camera : DHGameComponent
    {
        /// <summary>
        /// The state the camera is in.
        /// </summary>
        private enum CameraState
        {
            /// <summary>The camera is tracking a target.</summary>
            Tracking,
            /// <summary>The camera is scrolling to a location.</summary>
            Scrolling
        }
        /// <summary>
        /// Countdown timers for the camera.
        /// </summary>
        private enum Countdowns
        {
            /// <summary>The time left before the camera stops scrolling.</summary>
            Scroll
        }

        /// <summary>
        /// The camera offset from the bottom of the map.
        /// </summary>
        public int CameraY { get; private set; }
        /// <summary>
        /// The current camera velocity.
        /// </summary>
        public int VelocityY { get; private set; }

        /// <summary>
        /// The camera's state.
        /// </summary>
        private CameraState _state;
        /// <summary>
        /// The object the camera is tracking.
        /// </summary>
        private CollisionBody _trackingBody;
        /// <summary>
        /// The location the camera is scrolling to.
        /// </summary>
        private (int From, int To) _scrollingState;

        /// <summary>
        /// Creates a new camera.
        /// </summary>
        /// <param name="game">The game the camera belongs to.</param>
        /// <param name="level">The level the camera belongs to.</param>
        /// <returns>The new camera.</returns>
        public Camera(DiamondHollowGame game, Level level) : base(game, level)
        {
            Level = level;
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// Camera snaps th the bottom and top of the map.
        ///
        /// Tracking mode:
        /// - The camera will keep the target in the middle 50% of the screen.
        /// - If the target is outside of this area, the camera will adjust.
        /// - The further the target is outside, the faster the camera will adjust.
        /// 
        /// 
        /// Scrolling mode:
        /// - Speed up at the start and slow down at the end of the scroll.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            switch (_state)
            {
                case CameraState.Tracking:
                    int screenHeight = Game.WindowHeight;
                    int delta = _trackingBody.Center.Y + (int)_trackingBody.Velocity.Y - CameraY - screenHeight / 2;

                    // The camera will keep the target in the middle 50% of the screen
                    // If the target is outside of this area, the camera will adjust
                    // The further the target is outside, the faster the camera will adjust
                    if (Math.Abs(VelocityY) < 2.5) VelocityY = 0;
                    VelocityY -= (int)(Math.Sign(VelocityY) * 2.5);
                    if (delta > screenHeight / 8) VelocityY = Math.Max((int)Math.Pow((delta - screenHeight / 8) / 25, 2), VelocityY);
                    else if (delta < -screenHeight / 4) VelocityY = Math.Min(-(int)Math.Pow((delta + screenHeight / 4) / -25, 2), VelocityY);
                    CameraY += VelocityY;

                    // Snap camera to bottom and top of map
                    int levelHeight = Level.MapHeight - Game.WindowHeight;
                    if (CameraY < 0) CameraY = 0;
                    else if (CameraY > levelHeight) CameraY = levelHeight;

                    break;

                case CameraState.Scrolling when IsCountdownDone((int)Countdowns.Scroll):
                    _state = CameraState.Tracking;
                    break;

                case CameraState.Scrolling:
                    // Speed up at the start and slow down at the end of the scroll
                    float t = 0.5f - (float)Math.Cos(GetCountdownProgress((int)Countdowns.Scroll) * Math.PI) / 2f;
                    CameraY = (int)(_scrollingState.From + (_scrollingState.To - _scrollingState.From) * t);
                    break;
            }
        }

        /// <summary>
        /// Switch to tracking mode.
        /// </summary>
        /// <param name="body">The object to track.</param>
        public void Track(CollisionBody body)
        {
            _state = CameraState.Tracking;
            _trackingBody = body;
        }

        /// <summary>
        /// Switch to scrolling mode.
        /// </summary>
        /// <param name="posY">The location to scroll to.</param>
        /// <param name="duration">The duration of the scroll.</param>
        /// <param name="callback">The callback to call when the scroll is complete.</param>
        public void Scroll(int posY, int duration, Action callback)
        {
            _state = CameraState.Scrolling;
            VelocityY = 0;
            _scrollingState = (CameraY, Math.Max(posY - Game.WindowHeight / 2, 0));
            CreateCountdown((int)Countdowns.Scroll, duration, false, 0, callback);
        }
    }
}