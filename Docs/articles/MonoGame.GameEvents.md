# Game Events

Most games can be split into three phases. The first phase is the initialization phase, where memory is allocated and game assets are loaded. Then comes the game loop, where the game logic is periodically updated and components are rendered on the screen. Finally, the game closes, and memory is freed.

MonoGame provides a way to handle these phases through the use of events.

Components that subclass the `GameComponent` class can be added to the game's `Components` list and if they override any of the following methods, they will be called when the appropriate event is fired.


### Initialization

```cs
public override void Initialize()
```

The Initialize method is called after the constructor but before the main game loop (Update/Draw). This is where you can query any required services and load any non-graphic-related content.


```cs
protected virtual void LoadContent()
```

The LoadContent method is used to load your game content. It is called only once per game, within the Initialize method, before the main game loop starts.


### Game loop

```cs
public virtual void Update(GameTime gameTime)
```

The Update method is called multiple times per second, and it is used to update your game state (checking for collisions, gathering input, playing audio, etc.).


```cs
public virtual void Draw(GameTime gameTime)
```

Similar to the Update method, the Draw method is also called multiple times per second. This, as the name suggests, is responsible for drawing content to the screen.


### De-initialization

```cs
protected virtual void UnloadContent()
```

The UnloadContent method is called before the game closes and is used to unload any user-allocated content.