using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    /// <summary>
    /// Helper function.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Fast resize and copy of a multi-dimensional array.
        /// </summary>
        /// <param name="array">The array to resize.</param>
        /// <param name="newColumns">The new number of columns.</param>
        /// <param name="newRows">The new number of rows.</param>
        /// <typeparam name="T">The type of the array.</typeparam>
        public static void ResizeArray<T>(ref T[,] array, int newColumns, int newRows)
        {
            var newArray = new T[newColumns, newRows];
            int rows = array.GetLength(1);
            int columns = array.GetLength(0);
            for (int x = 0; x < columns; x++)
                Array.Copy(array, x * rows, newArray, x * newRows, rows);
            array = newArray;
        }
    }

    /// <summary>
    /// `0b1234`:
    /// <code>
    /// 1---2
    /// |   |
    /// 4---3
    /// </code>
    /// </summary>
    [Flags]
    public enum Corner
    {
        /// <summary>The top two corners.</summary>
        Top = 0b1100,
        /// <summary>The right two corners.</summary>
        Right = 0b0110,
        /// <summary>The bottom two corners.</summary>
        Bottom = 0b0011,
        /// <summary>The left two corners.</summary>
        Left = 0b1001,
    }

    /// <summary>
    /// A set of extension function for swithing between coordinate systems.
    /// - Grid - A measurement in tiles, `0,0` is in the bottom left of the map, used for placing objects on the map.
    /// - Default - A measurement in pixels, `0,0` is in the bottom left of the screen, used for collision bodies.
    /// - Screen - A measurement in pixels, `0,0` is in the top left of the screen, takes the camera position into account, used for drawing.
    /// </summary>
    public static class CoordinateExtensions
    {
        /// <summary>
        /// A reference to the game, used for the window height and camera position.
        /// </summary>
        private static DiamondHollowGame _game;
        /// <summary>
        /// A random number generator, used in <see cref="DiamondHollow.CoordinateExtensions.RandomOffset"/>.
        /// </summary>
        private static Random _random;
        /// <summary>
        /// Size of a tile in pixels.
        /// </summary>
        private static Point _tileSize;
        /// <summary>
        /// Initialize the extension functions.
        /// </summary>
        /// <param name="game">The game, used for the window height and camera position.</param>
        /// <param name="tileSize">The size of a tile in pixels.</param>
        public static void Initialize(DiamondHollowGame game, Point tileSize) => (_game, _tileSize, _random) = (game, tileSize, new Random());

        /// <summary>
        /// The height of the window in pixels.
        /// </summary>
        private static int ScreenHeight => _game.WindowHeight;
        /// <summary>
        /// The camera position in pixels.
        /// </summary>
        private static int CameraOffset => _game.Level.Camera.CameraY;

        /// <summary>
        /// Convert a default position to a grid position.
        /// </summary>
        /// <param name="p">The default position.</param>
        /// <returns>The grid position.</returns>
        public static Point ToGrid(this Point p) => new(p.X / _tileSize.X - (p.X < 0 ? 1 : 0), p.Y / _tileSize.Y - (p.Y < 0 ? 1 : 0));
        /// <summary>
        /// Convert a grid position to a default position.
        /// </summary>
        /// <param name="p">The grid position.</param>
        /// <returns>The default position.</returns>
        public static Point FromGrid(this Point p) => new(p.X * _tileSize.X, p.Y * _tileSize.Y);
        /// <summary>
        /// Snap a default position to the grid.
        /// </summary>
        /// <param name="p">The default position.</param>
        /// <returns>The adjusted default position.</returns>
        public static Point SnapToGrid(this Point p) => FromGrid(ToGrid(p));

        /// <summary>
        /// Convert a default position to a grid position.
        /// </summary>
        /// <param name="v">The default position.</param>
        /// <returns>The grid position.</returns>
        public static Vector2 ToGrid(this Vector2 v) => new(v.X / _tileSize.X, v.Y / _tileSize.Y);
        /// <summary>
        /// Convert a grid position to a default position.
        /// </summary>
        /// <param name="v">The grid position.</param>
        /// <returns>The default position.</returns>
        public static Vector2 FromGrid(this Vector2 v) => new(v.X * _tileSize.X, v.Y * _tileSize.Y);
        /// <summary>
        /// Snap a default position to the grid.
        /// </summary>
        /// <param name="v">The default position.</param>
        /// <returns>The adjusted default position.</returns>
        public static Vector2 SnapToGrid(this Vector2 v) => FromGrid(ToGrid(v));

        /// <summary>
        /// Convert a default position to a screen position.
        /// </summary>
        /// <param name="p">The default position.</param>
        /// <returns>The screen position.</returns>
        public static Point ToScreen(this Point p) => new(p.X, ScreenHeight - p.Y + CameraOffset);
        /// <summary>
        /// Convert a default positioned rectangle to a screen positioned rectangle.
        /// </summary>
        /// <param name="p">The default position.</param>
        /// <returns>The screen position.</returns>
        public static Rectangle ToScreen(this Rectangle r) => new(r.X, ScreenHeight - r.Y - r.Height + CameraOffset, r.Width, r.Height);

        /// <summary>
        /// Offset a position by an amount on the X and Y axis.
        /// </summary>
        /// <param name="p">The position.</param>
        /// <param name="x">The amount to offset by on the X axis.</param>
        /// <param name="y">The amount to offset by on the Y axis.</param>
        /// <returns>The offset position.</returns>
        public static Point Offset(this Point p, int x, int y) => new(p.X + x, p.Y + y);
        /// <summary>
        /// Offset a position by an amount on the X axis.
        /// </summary>
        /// <param name="p">The position.</param>
        /// <param name="x">The amount to offset by on the X axis.</param>
        /// <returns>The offset position.</returns>
        public static Point OffsetX(this Point p, int x) => new(p.X + x, p.Y);
        /// <summary>
        /// Offset a position by an amount on the Y axis.
        /// </summary>
        /// <param name="p">The position.</param>
        /// <param name="y">The amount to offset by on the Y axis.</param>
        /// <returns>The offset position.</returns>
        public static Point OffsetY(this Point p, int y) => new(p.X, p.Y + y);
        /// <summary>
        /// Offset a rectangle by an amount on the X axis.
        /// </summary>
        /// <param name="r">The rectangle.</param>
        /// <param name="x">The amount to offset by on the X axis.</param>
        /// <returns>The offset rectangle.</returns>
        public static Rectangle OffsetX(this Rectangle r, int x) => new(r.X + x, r.Y, r.Width, r.Height);
        /// <summary>
        /// Offset a rectangle by an amount on the Y axis.
        /// </summary>
        /// <param name="r">The rectangle.</param>
        /// <param name="y">The amount to offset by on the Y axis.</param>
        /// <returns>The offset rectangle.</returns>
        public static Rectangle OffsetY(this Rectangle r, int y) => new(r.X, r.Y + y, r.Width, r.Height);

        /// <summary>
        /// Multiplies a position by a scalar.
        /// </summary>
        /// <param name="p">The position.</param>
        /// <param name="s">The scalar.</param>
        /// <returns>The scaled position.</returns>
        public static Point Scale(this Point p, int s) => new(p.X * s, p.Y * s);
        /// <summary>
        /// Scale a rectangle by a scalar in all directions.
        /// </summary>
        /// <param name="r">The rectangle.</param>
        /// <param name="s">The scalar.</param>
        /// <returns>The scaled rectangle.</returns>
        public static Rectangle Grow(this Rectangle r, int s) => new(r.X - s, r.Y - s, r.Width + s * 2, r.Height + s * 2);

        /// <summary>
        /// Create a rectangle from a position, using the <see cref="DiamondHollow.CoordinateExtensions._tileSize"/> as the width and height.
        /// </summary>
        /// <param name="p">The position.</param>
        /// <returns>The rectangle.</returns>
        public static Rectangle MakeTile(this Point p) => new(p, _tileSize);
        /// <summary>
        /// Half the size of a point.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>The half point.</returns>
        public static Point Half(this Point p) => new(p.X / 2, p.Y / 2);
        /// <summary>
        /// Get the length from the origin to a point.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>The length.</returns>
        public static float Length(this Point p) => (float)Math.Sqrt(p.X * p.X + p.Y * p.Y);
        /// <summary>
        /// Get the squared length from the origin to a point.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>The squared length.</returns>
        public static int LengthSquared(this Point p) => p.X * p.X + p.Y * p.Y;

        /// <summary>
        /// Return positions of the corners of the rectangle, can be masked to only select some of the corners.
        /// </summary>
        /// <param name="r">The rectangle to get the corners from.</param>
        /// <param name="mask">The mask of the corners to return.</param>
        /// <returns>The corners of the rectangle.</returns>
        public static IEnumerable<Point> Corners(this Rectangle r, Corner mask = Corner.Top | Corner.Right | Corner.Bottom | Corner.Left)
        {
            if (((int)mask & 0b1000) != 0) yield return new Point(r.Left, r.Bottom - 1);
            if (((int)mask & 0b0100) != 0) yield return new Point(r.Right - 1, r.Bottom - 1);
            if (((int)mask & 0b0010) != 0) yield return new Point(r.Right - 1, r.Top);
            if (((int)mask & 0b0001) != 0) yield return new Point(r.Left, r.Top);
        }

        /// <summary>
        /// Return a random point inside the circle.
        /// </summary>
        /// <param name="v">The center of the circle.</param>
        /// <param name="distance">The radius of the circle.</param>
        /// <returns>A random point inside the circle.</returns>
        public static Vector2 RandomOffset(this Vector2 v, float distance = 1f)
        {
            var angle = _random.NextDouble() * Math.PI * 2;
            var length = _random.NextDouble();
            return v + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (float)length * distance;
        }
        /// <summary>
        /// Return a random point inside the circle.
        /// A convenience function for <see cref="DiamondHollow.CoordinateExtensions.RandomOffset(Vector2, float)"/>.
        /// </summary>
        /// <param name="p">The center of the circle.</param>
        /// <param name="distance">The radius of the circle.</param>
        /// <returns>A random point inside the circle.</returns>
        public static Point RandomOffset(this Point p, float distance = 1f) => p.ToVector2().RandomOffset(distance).ToPoint();
    }

    /// <summary>
    /// Some extra rendering functions for drawing lines.
    /// </summary>
    public static class RendererExtensions
    {
        /// <summary>
        /// A white pixel, used for drawing lines.
        /// </summary>
        private static Texture2D _whitePixel;
        /// <summary>
        /// Create a white pixel, if it doesn't exist yet.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use.</param>
        /// <returns>The white pixel.</returns>
        private static Texture2D GetWhitePixel(SpriteBatch spriteBatch)
        {
            if (_whitePixel is null)
            {
                _whitePixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _whitePixel.SetData(new[] { Color.White });
            }
            return _whitePixel;
        }

        /// <summary>
        /// Draw a line.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use.</param>
        /// <param name="point1">The first point of the line.</param>
        /// <param name="point2">The second point of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }

        /// <summary>
        /// Draw a line.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use.</param>
        /// <param name="point">The first point of the line.</param>
        /// <param name="length">The length of the line.</param>
        /// <param name="angle">The angle of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GetWhitePixel(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// A enum of mouse buttons.
    /// </summary>
    public enum MouseButton
    {
        /// <summary>The left mouse button.</summary>
        Left,
        /// <summary>The right mouse button.</summary>
        Right,
        /// <summary>The middle mouse button.</summary>
        Middle
    }

    /// <summary>
    /// Parity functions from the keyboard class, which aren't available for the mouse for some reason.
    /// </summary>
    public static class MouseButtonExtensions
    {
        /// <summary>
        /// Get the state of a mouse button.
        /// </summary>
        /// <param name="state">The mouse state this is extending.</param>
        /// <param name="button">The button to get the state of.</param>
        /// <returns>The state of the button.</returns>
        private static ButtonState GetButtonState(this MouseState state, MouseButton button) => button switch
        {
            MouseButton.Left => state.LeftButton,
            MouseButton.Right => state.RightButton,
            MouseButton.Middle => state.MiddleButton,
            _ => throw new ArgumentException("Invalid mouse button"),
        };
        /// <summary>
        /// Check if a mouse button is pressed.
        /// </summary>
        /// <param name="state">The mouse state this is extending.</param>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button is pressed.</returns>
        public static bool IsButtonDown(this MouseState state, MouseButton button) => GetButtonState(state, button) == ButtonState.Pressed;
        /// <summary>
        /// Check if a mouse button is released.
        /// </summary>
        /// <param name="state">The mouse state this is extending.</param>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button is released.</returns>
        public static bool IsButtonUp(this MouseState state, MouseButton button) => GetButtonState(state, button) == ButtonState.Released;
    }
}

// To get rid of a weird error, at least on macOS
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}