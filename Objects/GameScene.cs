using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    // A class for parenting components to a scene
    // Since there is only one scene in this game, this class is not necessary
    public class GameScene : DHGameComponent
    {
        private bool? _state;   // If the scene is active
        private readonly List<DHGameComponent> Components;

        public GameScene(DiamondHollowGame game, Level level) : base(game, level)
        {
            _state = null;
            Components = new List<DHGameComponent>();
        }

        public void AddComponent(DHGameComponent component)
        {
            Components.Add(component);
            if (!Game.Components.Contains(component))
            {
                Game.Components.Add(component);
            }
        }

        public void RemoveComponent(DHGameComponent component)
        {
            Components.Remove(component);
            Game.Components.Remove(component);
        }

        public void AddComponents(params DHGameComponent[] components) => components.ToList().ForEach(AddComponent);
        public void RemoveComponents(params DHGameComponent[] components) => components.ToList().ForEach(RemoveComponent);

        virtual protected void GameSceneEnter() { }
        virtual protected void GameSceneExit() { }

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