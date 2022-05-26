using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    enum DrawingLayers
    {
        Background = 10,
        Collectibles = 20,
        Enemies = 30,
        Projectiles = 40,
        Foreground = 50,
        Particles = 60,
        Player = 100,
    }

    public class DiamondHollowGame : Game
    {
        public GraphicsDeviceManager Graphics;
        public SpriteBatch SpriteBatch;

        public Texture2D WhitePixel;
        public SpriteFont Menlo;
        public int TileSize = 50;
        public int WindowHeight => GraphicsDevice.Viewport.Height;
        public int WindowWidth => GraphicsDevice.Viewport.Width;

        public Level Level;

        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        public KeyboardState KeyboardState;
        public MouseState MouseState;
        private readonly Random _random;

        public DiamondHollowGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();
            _random = new Random();
        }

        protected override void Initialize()
        {
            Graphics.PreferredBackBufferWidth = 1400;
            Graphics.PreferredBackBufferHeight = 800;
            Graphics.IsFullScreen = false;
            Graphics.ApplyChanges();

            CoordinateExtensions.Initialize(this, new Point(TileSize, TileSize));

            Level = new Level(this, "Levels/Components");

            Components.Add(Level);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            WhitePixel = new Texture2D(GraphicsDevice, 1, 1);
            WhitePixel.SetData(new[] { Color.White });

            Menlo = Content.Load<SpriteFont>("Fonts/Menlo");
        }

        private readonly Dictionary<string, Texture2D> _textures = new();
        public Texture2D GetTexture(string name)
        {
            if (!_textures.ContainsKey(name)) _textures[name] = Content.Load<Texture2D>(name);
            return _textures[name];
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            SpriteBatch.Dispose();
            WhitePixel.Dispose();
        }

        public bool KeyPressed(Keys key) => KeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        public bool KeyReleased(Keys key) => KeyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key);
        public bool ButtonPressed(MouseButton button) => MouseState.IsButtonDown(button) && _previousMouseState.IsButtonUp(button);

        public float Random() => (float)_random.NextDouble();
        public bool Chance(float chance) => _random.NextDouble() < chance;
        public TElem Choice<TElem>(IEnumerable<TElem> sequence) => sequence.ElementAt(_random.Next(0, sequence.Count()));
        public TElem WeightedChoice<TElem>(IEnumerable<TElem> sequence, IEnumerable<int> weights)
        {
            int totalWeight = weights.Sum();
            float random = Random() * totalWeight;
            int currentWeight = 0;
            for (int i = 0; i < sequence.Count(); i++)
            {
                currentWeight += weights.ElementAt(i);
                if (currentWeight > random)
                    return sequence.ElementAt(i);
            }
            return sequence.Last();
        }

        protected override void Update(GameTime gameTime)
        {
            _previousKeyboardState = KeyboardState;
            _previousMouseState = MouseState;
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            if (KeyboardState.IsKeyDown(Keys.Escape)) Exit();

            base.Update(gameTime);
        }
    }
}
