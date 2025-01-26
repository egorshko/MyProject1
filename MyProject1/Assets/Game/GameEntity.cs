using Game.City;
using Game.LoadingScreen;
using Game.MenuScreen;
using Game.SceneLoading;
using Game.Space;
using Shared.Disposable;
using Shared.Reactive;
using UnityEngine;

namespace Game 
{
    public class GameEntity : BaseDisposable
    {
        public struct Ctx 
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public GameEntryPoint.Data Data;
        }

        private readonly Ctx _ctx;

        IReactiveValue<bool> _isShowMenu;

        private GameLogic _gameLogic;

        private SceneLoadingEntity _sceneLoadingEntity;

        private LoadingScreenEntryPoint _loadingScreen;
        private MenuScreenEntryPoint _menuScreen;

        public GameEntity(Ctx ctx)
        {
            _ctx = ctx;

            _isShowMenu = new ReactiveValue<bool>(false).AddTo(this);

            _gameLogic = new GameLogic(new GameLogic.Ctx 
            {
                OnUpdate = _ctx.OnUpdate,

                GetLoadingScreen = sceneName => _sceneLoadingEntity.LoadScene<LoadingScreenEntryPoint>(sceneName),
                GetMenuScreen = sceneName => _sceneLoadingEntity.LoadScene<MenuScreenEntryPoint>(sceneName),
                GetSpace = sceneName => _sceneLoadingEntity.LoadScene<SpaceEntryPoint>(sceneName),
                GetCity = sceneName => _sceneLoadingEntity.LoadScene<CityEntryPoint>(sceneName),

                UnloadScene = sceneName => _sceneLoadingEntity.UnloadScene(sceneName),
            }).AddTo(this);

            var onSceneLoading = new ReactiveCommand<(string sceneName, float progress)>().AddTo(this);
            var onSceneLoadingDone = new ReactiveCommand<string>().AddTo(this);
            var onSceneUnloading = new ReactiveCommand<(string sceneName, float progress)>().AddTo(this);
            var onSceneUnloadingDone = new ReactiveCommand<string>().AddTo(this);
            _sceneLoadingEntity = new SceneLoadingEntity(new SceneLoadingEntity.Ctx 
            {
                OnSceneLoading = onSceneLoading,
                OnSceneLoadingDone = onSceneLoadingDone,

                OnSceneUnloading = onSceneUnloading,
                OnSceneUnloadingDone = onSceneUnloadingDone,
            }).AddTo(this);

            AsyncInit();
        }

        private async void AsyncInit() 
        {
            _loadingScreen = await _gameLogic.GetLoadingScreen(new LoadingScreenEntryPoint.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
            });

            _loadingScreen.ShowImmediate();

            _menuScreen = await _gameLogic.GetMenuScreen(new MenuScreenEntryPoint.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
                IsShowMenu = _isShowMenu,
            });

            SetMainMenuButtons();

            await _loadingScreen.Hide();
        }

        private void SetMainMenuButtons() 
        {
            _menuScreen.SetButtons(MenuScreenType.Vertical, "Main menu", 
                null,
                ("Play", LoadSpace),
                ("Quit", Application.Quit));
        }

        private async void LoadSpace() 
        {
            await _loadingScreen.Show();
            await _gameLogic.UnloadAll();
            _ = await _gameLogic.GetSpace(new SpaceEntryPoint.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
                ShowMenu = ShowSpaceMenu,
                IsShowMenu = _isShowMenu,
                LoadCity = LoadCity,
            });
            _menuScreen.Hide();
            await _loadingScreen.Hide();
        }

        private void ShowSpaceMenu() 
        {
            _menuScreen.SetButtons(MenuScreenType.Vertical, "Space menu",
                _menuScreen.Hide,
                ("Continue", _menuScreen.Hide),
                ("Main menu", ToMainMenu));

            async void ToMainMenu()
            {
                await _loadingScreen.Show();
                await _gameLogic.UnloadAll();
                SetMainMenuButtons();
                await _loadingScreen.Hide();
            }
        }

        private async void LoadCity(GameObject data) 
        {
            await _loadingScreen.Show();
            await _gameLogic.UnloadAll();
            _ = await _gameLogic.GetCity(new CityEntryPoint.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
                ToSpace = LoadSpace,
            });
            await _loadingScreen.Hide();
        }
    }
}

