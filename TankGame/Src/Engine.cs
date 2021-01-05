﻿using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using TankGame.Src.Actors;
using TankGame.Src.Actors.Pawns.Player;
using TankGame.Src.Data;
using TankGame.Src.Data.Map;
using TankGame.Src.Events;
using TankGame.Src.Gui.RenderComponents;

namespace TankGame.Src
{
    internal class Engine
    {
        public string GameTitle { get; private set; }
        private bool ShouldQuit { get; set; }

        private uint WindowHeight { get; set; }
        private uint WindowWidth { get; set; }
        private RenderWindow Window { get; set; }

        private uint GameViewWidth { get; set; }
        private uint GameViewHeight { get; set; }
        private View GameView { get; set; }

        private uint HUDViewWidth { get; set; }
        private uint HUDViewHeight { get; set; }
        private View HUDView { get; set; }

        private uint MenuViewWidth { get; set; }
        private uint MenuViewHeight { get; set; }
        private View MenuView { get; set; }

        private InputHandler InputHandler { get; }
        private CollisionManager CollisionManager { get; set; }
        private HUD HUD { get; set; }
        private Menu Menu { get; set; }

        private HashSet<ITickable> Tickables { get; }
        private HashSet<IRenderable> Renderables { get; }

        private GameMap Map { get; set; }

        public Engine()
        {
            GameTitle = "Tank Game";
            ShouldQuit = false;
            Tickables = new HashSet<ITickable>();
            Renderables = new HashSet<IRenderable>();

            InitializeManagers();

            InitializeWindow();
            InitializeView();

            InputHandler = new InputHandler(Window);
            SetInputHandlers();

            RegisterEvents();

            Menu = new Menu();

            //StartGame(false);
        }

        public void Loop()
        {
            DateTime time1 = DateTime.Now;
            DateTime time2;

            while (Window.IsOpen && !ShouldQuit)
            {
                time2 = DateTime.Now;
                float deltaTime = (time2.Ticks - time1.Ticks) / 10000000f;

                Tick(deltaTime);
                Render(deltaTime);

                time1 = time2;
            }
        }

        private void Tick(float deltaTime)
        {
            foreach (ITickable tickable in Tickables.ToList())
            {
                tickable.Tick(deltaTime);
            }

            CollisionManager.Tick();
            GamestateManager.Instance.Tick(deltaTime);
        }

        private void Render(float deltaTime)
        {
            Window.DispatchEvents();
            Window.SetView(GameView);
            Window.Clear(Color.Black);

            if (GamestateManager.Instance.Player != null) RecenterView(GamestateManager.Instance.Player.RealPosition);

            Renderables.ToList()
                .FindAll(renderable => renderable.RenderableRenderView == RenderView.Game && renderable.Visible)
                .OrderBy(renderable => (int)renderable.RenderableRenderLayer)
                .ToList()
                .ForEach((IRenderable renderable)
                    => renderable.GetRenderComponents().ToList().ForEach((IRenderComponent renderComponent)
                        => Window.Draw(renderComponent.Shape, new RenderStates(shader: (renderable is IShadable shadable ? shadable.CurrentShader : null)))));

            Window.SetView(HUDView);

            Renderables.ToList()
               .FindAll(renderable => renderable.RenderableRenderView == RenderView.HUD && renderable.Visible)
               .OrderBy(renderable => (int)renderable.RenderableRenderLayer)
               .ToList()
               .ForEach((IRenderable renderable)
                   => renderable.GetRenderComponents().ToList().ForEach((IRenderComponent renderComponent)
                       => Window.Draw(renderComponent.Shape, new RenderStates(shader: (renderable is IShadable shadable ? shadable.CurrentShader : null)))));
            
            Window.SetView(MenuView);

            Renderables.ToList()
               .FindAll(renderable => renderable.RenderableRenderView == RenderView.Menu && renderable.Visible)
               .OrderBy(renderable => (int)renderable.RenderableRenderLayer)
               .ToList()
               .ForEach((IRenderable renderable)
                   => renderable.GetRenderComponents().ToList().ForEach((IRenderComponent renderComponent)
                       => Window.Draw(renderComponent.Shape, new RenderStates(shader: (renderable is IShadable shadable ? shadable.CurrentShader : null)))));

            Window.Display();
        }

        private void StartGame(bool isNewGame)
        {
            Menu.Hide();

            HUD = new HUD();

            GamestateManager.Instance.Start();
            GamestateManager.Instance.Map = new GameMap(isNewGame);
            Map = GamestateManager.Instance.Map;
        }

        private void InitializeManagers()
        {
            CollisionManager = new CollisionManager();
            TextureManager.Initialize();
            SoundManager.Initialize();
            KeyManager.Initialize();
            MusicManager.Initialize();
        }

        private void InitializeWindow()
        {
            WindowWidth = 800;
            WindowHeight = WindowWidth * 16 / 10;

            Window = new RenderWindow(new VideoMode(WindowHeight, WindowWidth), GameTitle, Styles.Default, new ContextSettings() { AntialiasingLevel = 2 });
            Window.SetVerticalSyncEnabled(true);
            Window.Closed += (_, __) => Window.Close();
        }

        private void InitializeView()
        {
            GameViewWidth = 64 * (2 * 6 + 1);
            GameViewHeight = GameViewWidth;

            HUDViewWidth = 1000;
            HUDViewHeight = HUDViewWidth / 5;

            MenuViewWidth = 1000;
            MenuViewHeight = MenuViewWidth;

            GameView = new View(new Vector2f(GameViewWidth / 2, GameViewHeight / 2), new Vector2f(GameViewWidth, GameViewHeight));
            HUDView = new View(new Vector2f(HUDViewWidth / 2 - 32, HUDViewHeight / 2 - 32), new Vector2f(HUDViewWidth, HUDViewHeight));
            MenuView = new View(new Vector2f(MenuViewWidth / 2, MenuViewHeight / 2), new Vector2f(MenuViewWidth, MenuViewHeight));

            RecalculateViewport(WindowWidth, WindowHeight);
        }

        private void SetInputHandlers()
        {
            if (Window != null && InputHandler != null)
            {
                Window.KeyPressed += InputHandler.OnKeyPress;
                Window.MouseButtonPressed += InputHandler.OnClick;
                Window.Resized += OnResize;
            }
        }

        private void OnResize(object sender, SizeEventArgs newSize)
        {
            WindowWidth = newSize.Height;
            WindowHeight = newSize.Width;
            RecalculateViewport(newSize.Height, newSize.Width);
        }

        private void RecalculateViewport(uint height, uint width)
        {
            float uiWidth = 1F;
            float uiHeight = 0.2F;
            float gameSize = 0.8F;
            float aspectRatio = (float)height / width;
            
            if (GameView != null)
            {
                GameView.Viewport = Window.Size.X > Window.Size.Y
                    ? new FloatRect(new Vector2f((1- gameSize) + ((gameSize - (gameSize * aspectRatio)) / 2), (1 - gameSize)), new Vector2f(gameSize * aspectRatio, gameSize))
                    : new FloatRect(new Vector2f((1 - gameSize), (1 - gameSize) + ((gameSize - (gameSize * (1 / aspectRatio))) / 2)), new Vector2f(gameSize, gameSize * (1 / aspectRatio)));
            }

            if (HUDView != null)
            {
                HUDView.Viewport = Window.Size.X > Window.Size.Y
                   ? new FloatRect(new Vector2f((1 - uiWidth * aspectRatio) / 2, 0), new Vector2f(uiWidth * aspectRatio, uiHeight))
                   : new FloatRect(new Vector2f((1 - uiWidth) / 2, 0), new Vector2f(uiWidth, uiHeight * (1 / aspectRatio)));
            }

            if (MenuView != null)
            {
                MenuView.Viewport = Window.Size.X > Window.Size.Y
                    ? new FloatRect(new Vector2f((1 - aspectRatio) / 2, 0), new Vector2f(aspectRatio, 1))
                    : new FloatRect(new Vector2f(0, ( 1 - (1 / aspectRatio))/ 2), new Vector2f(1, 1 / aspectRatio));
            }
        }

        private void RecenterView(Vector2f position)
        {
            if (GameView != null) GameView.Center = position;
        }

        private void RegisterEvents()
        {            
            MessageBus messageBus = MessageBus.Instance;

            messageBus.Register(MessageType.StartGame, OnStartGame);
            messageBus.Register(MessageType.Quit, OnQuit);

            messageBus.Register(MessageType.RegisterTickable, OnRegisterTickable);
            messageBus.Register(MessageType.UnregisterTickable, OnUnregisterTickable);

            messageBus.Register(MessageType.RegisterRenderable, OnRegisterRenderable);
            messageBus.Register(MessageType.UnregisterRenderable, OnUnregisterRenderable);

            messageBus.Register(MessageType.PlayerMoved, OnPlayerMoved);
        }

        private void OnQuit(object sender, EventArgs eventArgs) => ShouldQuit = true;

        private void OnRegisterTickable(object sender, EventArgs eventArgs)
        {
            if (sender is ITickable tickable) Tickables.Add(tickable);
        }

        private void OnUnregisterTickable(object sender, EventArgs eventArgs)
        {
            if (sender is ITickable tickable) Tickables.Remove(tickable);
        }

        private void OnRegisterRenderable(object sender, EventArgs eventArgs)
        {
            if (sender is IRenderable renderable) Renderables.Add(renderable);
        }

        private void OnUnregisterRenderable(object sender, EventArgs eventArgs)
        {
            if (sender is IRenderable renderable) Renderables.Remove(renderable);
        }
        
        private void OnPlayerMoved(object sender, EventArgs eventArgs)
        {
            if (sender is Player senderPlayer)
            {
                RecenterView(senderPlayer.RealPosition);
            }
        }

        private void OnStartGame(object sender, EventArgs eventArgs)
        {
            if (eventArgs is StartGameEventArgs startGameEventArgs) StartGame(startGameEventArgs.NewGame);
        }
    }
}