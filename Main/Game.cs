using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace DiamondHollow
{
    // Order in which different groups of components are drawn
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

    // The main class for the game
    //     Creates the game window
    //     Handles input and randomness
    // 
    // All object in the game are subclasses of GameComponent and when they are added to this game's components collection,
    // their virtual methods can be overridden and will be called at specific times automatically.
    public class DiamondHollowGame : Game
    {
        public GraphicsDeviceManager Graphics;  // Used only to create & control the game window
        public SpriteBatch SpriteBatch;         // Used for optimized drawing of sprites & text

        public Texture2D WhitePixel;    // A 1x1 pixel texture for drawing single colored rectangles
        public SpriteFont Menlo;        // A font for drawing text
        public int TileSize = 50;       // Size of building block for the game
        public int WindowHeight => GraphicsDevice.Viewport.Height;
        public int WindowWidth => GraphicsDevice.Viewport.Width; 

        public Level Level;     // Stores the map, player, enemies, collectibles, etc.

        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        public KeyboardState KeyboardState;
        public MouseState MouseState;
        private readonly Random _random;

        public DiamondHollowGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";  // The path to game resources
            IsMouseVisible = true;
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();
            _random = new Random();
        }

        // Called when the game is first created, almost right after the constructor, after the graphics device is created.
        protected override void Initialize()
        {
            Graphics.PreferredBackBufferWidth = 1400;
            Graphics.PreferredBackBufferHeight = 800;
            Graphics.IsFullScreen = false;
            Graphics.ApplyChanges();

            // A set of helper methods, refer to the implementation for more details (Other/Helpers.cs)
            CoordinateExtensions.Initialize(this, new Point(TileSize, TileSize));

            Level = new Level(this, "Levels/Components");

            // Components are registered for automatic update, draw, load, etc. calls in this way
            Components.Add(Level);

            // ! Always call the base class's method
            base.Initialize();
        }

        // Called after the game is initialized, but before the game is first drawn, used to load (larger) resources
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            WhitePixel = new Texture2D(GraphicsDevice, 1, 1);
            WhitePixel.SetData(new[] { Color.White });

            Menlo = Content.Load<SpriteFont>("Fonts/Menlo");    // Fonts/Menlo.spritefont is an XML file under the resource path which defines what font to load, the actual font is provided by the operating system
        }

        // A lazy way to prevent multiple instances of the same game component from wasting memory by loading the same texture multiple times
        private readonly Dictionary<string, Texture2D> _textures = new();
        public Texture2D GetTexture(string name)
        {
            if (!_textures.ContainsKey(name)) _textures[name] = Content.Load<Texture2D>(name);
            return _textures[name];
        }

        // Called when the game is being closed, used to unload manually allocated resources
        protected override void UnloadContent()
        {
            base.UnloadContent();

            SpriteBatch.Dispose();
            WhitePixel.Dispose();
        }

        // Helpers to check for input changes
        public bool KeyPressed(Keys key) => KeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        public bool KeyReleased(Keys key) => KeyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key);
        public bool ButtonPressed(MouseButton button) => MouseState.IsButtonDown(button) && _previousMouseState.IsButtonUp(button);

        // Unified random generation - seed can be specified to get the same random sequence each time
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

        // Called by default a maximum of 60 times per second, used to update the game state
        // gameTime contains elapsed time since last update 
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
