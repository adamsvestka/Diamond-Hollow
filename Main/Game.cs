using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace DiamondHollow
{
    /// <summary>
    /// Order in which different groups of components are drawn.
    /// </summary>
    enum DrawingLayers
    {
        /// <summary>Used by <see cref="DiamondHollow.Level"/> and <see cref="DiamondHollow.RespawnAnchor"/>.</summary>
        Background = 10,
        /// <summary>Used by <see cref="DiamondHollow.Diamond"/>, <see cref="DiamondHollow.LargeHeart"/>, <see cref="DiamondHollow.Nuke"/> and <see cref="DiamondHollow.SmallHeart"/>.</summary>
        Collectibles = 20,
        /// <summary>Used by <see cref="DiamondHollow.Bird"/>, <see cref="DiamondHollow.CeilingShooter"/>, <see cref="DiamondHollow.Slime"/>, <see cref="DiamondHollow.Spider"/> and <see cref="DiamondHollow.WallShooter"/>.</summary>
        Enemies = 30,
        /// <summary>Used by <see cref="DiamondHollow.Projectile"/>.</summary>
        Projectiles = 40,
        /// <summary>Used by <see cref="DiamondHollow.Foreground"/>.</summary>
        Foreground = 50,
        /// <summary>Used by <see cref="DiamondHollow.ParticleInstance"/>.</summary>
        Particles = 60,
        /// <summary>Used by <see cref="DiamondHollow.Player"/>.</summary>
        Player = 100,
    }

    /// <summary>
    /// The main class for the game.
    /// - Creates the game window.
    /// - Handles input and randomness.
    /// 
    /// All object in the game are subclasses of <see cref="Microsoft.Xna.Framework.GameComponent"/>  and when they are added to this game's components collection,
    /// their virtual methods can be overridden and will be called at specific times automatically.
    /// </summary>
    public class DiamondHollowGame : Game
    {
        /// <summary>
        /// Used only to create &amp; control the game window.
        /// </summary>
        public GraphicsDeviceManager Graphics;
        /// <summary>
        /// Used for optimized drawing of sprites &amp; text.
        /// </summary>
        public SpriteBatch SpriteBatch;

        /// <summary>
        /// A 1x1 pixel texture for drawing single colored rectangles.
        /// </summary>
        public Texture2D WhitePixel;
        /// <summary>
        /// A font for drawing text.
        /// </summary>
        public SpriteFont Menlo;
        /// <summary>
        /// Size of building block for the game.
        /// </summary>
        public int TileSize = 50;
        /// <summary>
        /// Height of the game window.
        /// </summary>
        public int WindowHeight => GraphicsDevice.Viewport.Height;
        /// <summary>
        /// Width of the game window.
        /// </summary>
        public int WindowWidth => GraphicsDevice.Viewport.Width;

        /// <summary>
        /// Stores the map, player, enemies, collectibles, etc.
        /// </summary>
        public Level Level;

        /// <summary>
        /// Stores the previous state of the keyboard, to detect key presses.
        /// </summary>
        private KeyboardState _previousKeyboardState;
        /// <summary>
        /// Stores the previous state of the mouse, to detect mouse clicks.
        /// </summary>
        private MouseState _previousMouseState;
        /// <summary>
        /// The current state of the keyboard.
        /// </summary>
        public KeyboardState KeyboardState;
        /// <summary>
        /// The current state of the mouse.
        /// </summary>
        public MouseState MouseState;
        /// <summary>
        /// A random number generator.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        /// Create a new game.
        /// </summary>
        public DiamondHollowGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";  // The path to game resources
            IsMouseVisible = true;
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();
            _random = new Random();
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.Game.Initialize"/>
        /// <summary>
        /// Called when the game is first created, almost right after the constructor, after the graphics device is created.
        /// </summary>
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

        // <inheritdoc cref="Microsoft.Xna.Framework.Game.LoadContent"/>
        /// <summary>
        /// Called after the game is initialized, but before the game is first drawn, used to load (larger) resources.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            WhitePixel = new Texture2D(GraphicsDevice, 1, 1);
            WhitePixel.SetData(new[] { Color.White });

            Menlo = Content.Load<SpriteFont>("Fonts/Menlo");    // Fonts/Menlo.spritefont is an XML file under the resource path which defines what font to load, the actual font is provided by the operating system
        }

        /// <summary>
        /// A cache for loaded textures.
        /// </summary>
        /// <seealso cref="DiamondHollow.DiamondHollowGame.GetTexture"/>
        private readonly Dictionary<string, Texture2D> _textures = new();
        /// <summary>
        /// Load a texture from the content manager.
        /// A lazy way to prevent multiple instances of the same game component from wasting memory by loading the same texture multiple times.
        /// </summary>
        /// <param name="name">The filename of the texture.</param>
        /// <returns>The loaded texture.</returns>
        /// <seealso cref="DiamondHollow.DiamondHollowGame._textures"/>
        public Texture2D GetTexture(string name)
        {
            if (!_textures.ContainsKey(name)) _textures[name] = Content.Load<Texture2D>(name);
            return _textures[name];
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.Game.UnloadContent"/>
        /// <summary>
        /// Called when the game is being closed, used to unload manually allocated resources.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();

            SpriteBatch.Dispose();
            WhitePixel.Dispose();
        }

        /// <summary>
        /// Check if a key was just pressed.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key was just pressed, false otherwise.</returns>
        public bool KeyPressed(Keys key) => KeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        /// <summary>
        /// Check if a key was just released.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key was just released, false otherwise.</returns>
        public bool KeyReleased(Keys key) => KeyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key);
        /// <summary>
        /// Check if a mouse button was just pressed.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button was just pressed, false otherwise.</returns>
        public bool ButtonPressed(MouseButton button) => MouseState.IsButtonDown(button) && _previousMouseState.IsButtonUp(button);

        /// <summary>
        /// Unified random generation - seed can be specified to get the same random sequence each time
        /// </summary>
        /// <returns>A random number.</returns>
        public float Random() => (float)_random.NextDouble();
        /// <summary>
        /// Generate true or false with a set probability.
        /// </summary>
        /// <param name="chance">The probability of true, between 0 and 1.</param>
        /// <returns>True or false.</returns>
        public bool Chance(float chance) => _random.NextDouble() < chance;
        /// <summary>
        /// Choose a random element from a list with equal probability.
        /// </summary>
        /// <param name="sequence">The list of elements to choose from.</param>
        /// <typeparam name="TElem">The type of the elements in the list.</typeparam>
        /// <returns>A random element from the list.</returns>
        public TElem Choice<TElem>(IEnumerable<TElem> sequence) => sequence.ElementAt(_random.Next(0, sequence.Count()));
        /// <summary>
        /// Choose a random element from a list with set probabilities
        /// </summary>
        /// <param name="sequence">The list of elements to choose from.</param>
        /// <param name="weights">The weights of the elements.</param>
        /// <typeparam name="TElem">The type of the elements in the list.</typeparam>
        /// <returns>A random element from the list.</returns>
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

        // <inheritdoc cref="Microsoft.Xna.Framework.Game.Update"/>
        /// <summary>
        /// Called by default a maximum of 60 times per second, used to update the game state.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            _previousKeyboardState = KeyboardState;
            _previousMouseState = MouseState;
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            // if (KeyboardState.IsKeyDown(Keys.Escape)) Exit();

            base.Update(gameTime);
        }
    }
}
