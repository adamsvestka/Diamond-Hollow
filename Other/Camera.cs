using System;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    public class Camera
    {
        private static GraphicsDevice _screen;
        private static Level _level;

        public int CameraY { get; private set; }
        public int VelocityY { get; private set; }

        public Camera(GraphicsDevice screen, Level level)
        {
            _screen = screen;
            _level = level;
        }

        public void Track(int pos_y, int vel_y)
        {
            int screen_height = _screen.Viewport.Height;
            int delta = pos_y + vel_y - CameraY - screen_height / 2;

            if (Math.Abs(VelocityY) < 2.5) VelocityY = 0;
            VelocityY -= (int)(Math.Sign(VelocityY) * 2.5);
            if (delta > screen_height / 8) VelocityY = Math.Max((int)Math.Pow((delta - screen_height / 8) / 25, 2), VelocityY);
            else if (delta < -screen_height / 4) VelocityY = Math.Min(-(int)Math.Pow((delta + screen_height / 4) / -25, 2), VelocityY);
            CameraY += VelocityY;

            int level_height = _level.GetHeight() - _screen.Viewport.Height;
            if (CameraY < 0) CameraY = 0;
            else if (CameraY > level_height) CameraY = level_height;
        }
    }
}