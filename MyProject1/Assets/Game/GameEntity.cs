using Game.SceneLoading;
using Game.World;
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

        private IReactiveValue<bool> _isShowMenu;
        private IReactiveValue<Vector3> _spacePos;
        private IReactiveValue<Quaternion> _spaceRot;

        private GameLogic _gameLogic;

        private SceneLoadingEntity _sceneLoadingEntity;

        private LoadingScreen.LoadingScreen _loadingScreen;
        private MenuScreen.MenuScreen _menuScreen;

        public GameEntity(Ctx ctx)
        {
            _ctx = ctx;

            _isShowMenu = new ReactiveValue<bool>(false).AddTo(this);

            //TODO load spacePos and spaceRot here...
            _spacePos = new ReactiveValue<Vector3>().AddTo(this);
            _spaceRot = new ReactiveValue<Quaternion>().AddTo(this);

            _gameLogic = new GameLogic(new GameLogic.Ctx 
            {
                OnUpdate = _ctx.OnUpdate,

                GetLoadingScreen = sceneName => _sceneLoadingEntity.LoadScene<LoadingScreen.LoadingScreen>(sceneName),
                GetMenuScreen = sceneName => _sceneLoadingEntity.LoadScene<MenuScreen.MenuScreen>(sceneName),

                GetWorld = sceneName => _sceneLoadingEntity.LoadScene<WorldEntryPoint>(sceneName),

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
            _loadingScreen = await _gameLogic.GetLoadingScreen(new LoadingScreen.LoadingScreen.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
            });
            _loadingScreen.ShowImmediate();
            _menuScreen = await _gameLogic.GetMenuScreen(new MenuScreen.MenuScreen.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
                IsShowMenu = _isShowMenu,
            });
            SetMainMenuButtons();
            await _loadingScreen.Hide();
        }

        private void SetMainMenuButtons() 
        {
            _menuScreen.SetButtons(MenuScreen.MenuScreen.MenuScreenType.Vertical, "Main menu", 
                null,
                ("World", () => LoadWorld((WorldEntryPoint.World.World_Test_0, Vector3.zero, Vector3.zero))),
                ("Quit", Application.Quit));
        }

        private void ShowSpaceMenu() 
        {
            _menuScreen.SetButtons(MenuScreen.MenuScreen.MenuScreenType.Vertical, "Space menu",
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

        private async void LoadWorld((WorldEntryPoint.World world, Vector3 worldPos, Vector3 worldRot) data)
        {
            await _loadingScreen.Show();
            await _gameLogic.UnloadAll();

            _ = await _gameLogic.GetWorld(data.world, new WorldEntryPoint.Ctx
            {
                WorldPos = data.worldPos,
                WorldRot = Quaternion.Euler(data.worldRot),
                OnUpdate = _ctx.OnUpdate,
                ShowMenu = ShowSpaceMenu,
                IsShowMenu = _isShowMenu,
                LoadWorld = LoadWorld,
            });
            _menuScreen.Hide();
            await _loadingScreen.Hide();
        }
    }
}

