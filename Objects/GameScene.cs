using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// A class for parenting components to a scene.
    /// Since there is only one scene in this game, this class is not necessary.
    /// </summary>
    public class GameScene : DHGameComponent
    {
        /// <summary>
        /// Whether the scene is currently active.
        /// </summary>
        private bool? _state;
        /// <summary>
        /// The list of components in this scene.
        /// </summary>
        private readonly List<DHGameComponent> Components;

        /// <summary>
        /// Create a new scene.
        /// </summary>
        /// <param name="game">The game this scene is attached to.</param>
        /// <param name="level">The level this scene is attached to.</param>
        /// <returns>The new scene.</returns>
        public GameScene(DiamondHollowGame game, Level level) : base(game, level)
        {
            _state = null;
            Components = new List<DHGameComponent>();
        }

        /// <summary>
        /// Add a component to this scene.
        /// </summary>
        /// <param name="component">The component to add.</param>
        public void AddComponent(DHGameComponent component)
        {
            Components.Add(component);
            if (!Game.Components.Contains(component))
            {
                Game.Components.Add(component);
            }
        }

        /// <summary>
        /// Remove a component from this scene.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        public void RemoveComponent(DHGameComponent component)
        {
            Components.Remove(component);
            Game.Components.Remove(component);
        }

        /// <summary>
        /// Bulk add components to this scene.
        /// </summary>
        /// <param name="components">The components to add.</param>
        public void AddComponents(params DHGameComponent[] components) => components.ToList().ForEach(AddComponent);
        /// <summary>
        /// Bulk remove components from this scene.
        /// </summary>
        /// <param name="components">The components to remove.</param>
        public void RemoveComponents(params DHGameComponent[] components) => components.ToList().ForEach(RemoveComponent);

        /// <summary>
        /// Called when the scene is activated.
        /// </summary>
        virtual protected void GameSceneEnter() { }
        /// <summary>
        /// Called when the scene is deactivated.
        /// </summary>
        virtual protected void GameSceneExit() { }

        /// <summary>
        /// A wrapper for <see cref="DiamondHollow.GameScene._state"/>.
        /// Uses <see cref="Microsoft.Xna.Framework.GameComponent.Enabled"/> to regulate the components' <see cref="Microsoft.Xna.Framework.GameComponent.Update"/> methods and
        /// <see cref="Microsoft.Xna.Framework.DrawableGameComponent.Visible"/> to regulate the components' <see cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/> methods.
        /// </summary>
        /// <value>Default: false</value>
        /// <seealso cref="DiamondHollow.GameScene.GameSceneEnter"/>
        /// <seealso cref="DiamondHollow.GameScene.GameSceneExit"/>
        public bool State
        {
            get => _state ?? false;
            set
            {
                if (_state == value) return;

                _state = value;
                if (value) GameSceneEnter();
                else GameSceneExit();

                Enabled = value;    // Enables/disables the component's Update method
                Visible = value;    // Enables/disables the component's Draw method
                foreach (var component in Components)
                {
                    component.Enabled = value;
                    if (component is DrawableGameComponent drawableComponent)
                    {
                        drawableComponent.Visible = value;
                    }
                }
            }
        }
    }
}