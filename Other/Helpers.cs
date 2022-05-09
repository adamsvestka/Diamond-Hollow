using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    [Flags]
    public enum Corner
    {
        Top = 0b1100,
        Right = 0b0110,
        Bottom = 0b0011,
        Left = 0b1001,
    }

    public static class CoordinateExtensions
    {
        private static DiamondHollowGame _game;
        private static Point _tileSize;
        public static void Initialize(DiamondHollowGame game, Point tileSize) => (_game, _tileSize) = (game, tileSize);

        private static int ScreenHeight => _game.GraphicsDevice.Viewport.Height;
        private static int CameraOffset => _game.Camera.CameraY;

        public static Point ToGrid(this Point p) => new(p.X / _tileSize.X - (p.X < 0 ? 1 : 0), p.Y / _tileSize.Y - (p.Y < 0 ? 1 : 0));
        public static Point FromGrid(this Point p) => new(p.X * _tileSize.X, p.Y * _tileSize.Y);
        public static Point SnapToGrid(this Point p) => FromGrid(ToGrid(p));

        public static Point ToScreen(this Point p) => new(p.X, ScreenHeight - p.Y + CameraOffset);
        public static Rectangle ToScreen(this Rectangle r) => new(r.X, ScreenHeight - r.Y - r.Height + CameraOffset, r.Width, r.Height);

        public static Point Offset(this Point p, int x, int y) => new(p.X + x, p.Y + y);
        public static Point OffsetX(this Point p, int x) => new(p.X + x, p.Y);
        public static Point OffsetY(this Point p, int y) => new(p.X, p.Y + y);
        public static Rectangle Offset(this Rectangle r, Point p) => new(r.X + p.X, r.Y + p.Y, r.Width, r.Height);
        public static Rectangle Offset(this Rectangle r, int x, int y) => new(r.X + x, r.Y + y, r.Width, r.Height);
        public static Rectangle OffsetX(this Rectangle r, int x) => new(r.X + x, r.Y, r.Width, r.Height);
        public static Rectangle OffsetY(this Rectangle r, int y) => new(r.X, r.Y + y, r.Width, r.Height);

        public static Rectangle MakeTile(this Point p) => new(p, _tileSize);

        public static IEnumerable<Point> Corners(this Rectangle r, Corner mask = Corner.Top | Corner.Right | Corner.Bottom | Corner.Left)
        {
            if (((int)mask & 0b1000) != 0) yield return new Point(r.Left, r.Bottom - 1);
            if (((int)mask & 0b0100) != 0) yield return new Point(r.Right - 1, r.Bottom - 1);
            if (((int)mask & 0b0010) != 0) yield return new Point(r.Right - 1, r.Top);
            if (((int)mask & 0b0001) != 0) yield return new Point(r.Left, r.Top);
        }
    }

    public static class RendererExtensions
    {
        private static Texture2D _whitePixel;
        private static Texture2D GetWhitePixel(SpriteBatch spriteBatch)
        {
            if (_whitePixel is null)
            {
                _whitePixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _whitePixel.SetData(new[] { Color.White });
            }
            return _whitePixel;
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance * 2, angle, color, thickness);
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GetWhitePixel(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        }
    }

    public enum MouseButton { Left, Right, Middle }

    public static class MouseButtonExtensions
    {
        private static ButtonState GetButtonState(this MouseState state, MouseButton button) => button switch {
            MouseButton.Left => state.LeftButton,
            MouseButton.Right => state.RightButton,
            MouseButton.Middle => state.MiddleButton,
            _ => throw new ArgumentException("Invalid mouse button"),
        };
        public static bool IsButtonDown(this MouseState state, MouseButton button) => GetButtonState(state, button) == ButtonState.Pressed;
        public static bool IsButtonUp(this MouseState state, MouseButton button) => GetButtonState(state, button) == ButtonState.Released;
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}