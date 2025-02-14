using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Game 
{
    public class Game : BaseDisposableMB
    {
        private class Entity : BaseDisposable
        {
            private class Logic : BaseDisposable
            {
                public struct Ctx
                {
                    public IReadOnlyReactiveCommand<float> OnUpdate;

                    public Func<string, UniTask<LoadingScreen.LoadingScreen>> GetLoadingScreen;
                    public Func<string, UniTask<MenuScreen.MenuScreen>> GetMenuScreen;
                    public Func<string, UniTask<World.World>> GetWorld;
                    public Func<string, UniTask> UnloadScene;
                }

                private readonly Ctx _ctx;

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;
                }

                public async UniTask<LoadingScreen.LoadingScreen> GetLoadingScreen(LoadingScreen.LoadingScreen.Ctx ctx)
                {
                    const string loadingScreenName = "LoadingScreen";
                    var loadingScreenEntryPoint = await _ctx.GetLoadingScreen.Invoke(loadingScreenName);
                    loadingScreenEntryPoint.Init(ctx);
                    new DisposeObserver(async () => await _ctx.UnloadScene(loadingScreenName)).AddTo(this);
                    return loadingScreenEntryPoint;
                }

                public async UniTask<MenuScreen.MenuScreen> GetMenuScreen(MenuScreen.MenuScreen.Ctx ctx)
                {
                    const string menuScreenName = "MenuScreen";
                    var menuScreenEntryPoint = await _ctx.GetMenuScreen.Invoke(menuScreenName);
                    menuScreenEntryPoint.Init(ctx);
                    new DisposeObserver(async () => await _ctx.UnloadScene(menuScreenName)).AddTo(this);
                    return menuScreenEntryPoint;
                }

                public async UniTask<World.World> GetWorld(World.World.WorldName world, World.World.Ctx ctx)
                {
                    var worldEntryPoint = await _ctx.GetWorld.Invoke(world.ToString());
                    worldEntryPoint.Init(ctx);
                    return worldEntryPoint;
                }

                private async UniTask UnloadWorlds()
                {
                    var worlds = Enum.GetNames(typeof(World.World.WorldName));
                    foreach (var worldName in worlds)
                        await _ctx.UnloadScene(worldName);
                }

                public async UniTask UnloadAll()
                {
                    await UnloadWorlds();
                }

                protected override async UniTask OnAsyncDispose()
                {
                    await UnloadAll();
                    await base.OnAsyncDispose();
                }
            }

            public struct Ctx
            {
                public IReadOnlyReactiveCommand<float> OnUpdate;
            }

            private readonly Ctx _ctx;

            private IReactiveValue<bool> _isShowMenu;
            private IReactiveValue<Vector3> _spacePos;
            private IReactiveValue<Quaternion> _spaceRot;

            private Logic _gameLogic;

            private SceneLoading.SceneLoading.Entity _sceneLoadingEntity;

            private LoadingScreen.LoadingScreen _loadingScreen;
            private MenuScreen.MenuScreen _menuScreen;

            public Entity(Ctx ctx)
            {
                _ctx = ctx;

                _isShowMenu = new ReactiveValue<bool>(false).AddTo(this);

                //TODO load spacePos and spaceRot here...
                _spacePos = new ReactiveValue<Vector3>().AddTo(this);
                _spaceRot = new ReactiveValue<Quaternion>().AddTo(this);

                _gameLogic = new Logic(new Logic.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,

                    GetLoadingScreen = sceneName => _sceneLoadingEntity.LoadScene<LoadingScreen.LoadingScreen>(sceneName),
                    GetMenuScreen = sceneName => _sceneLoadingEntity.LoadScene<MenuScreen.MenuScreen>(sceneName),

                    GetWorld = sceneName => _sceneLoadingEntity.LoadScene<World.World>(sceneName),

                    UnloadScene = sceneName => _sceneLoadingEntity.UnloadScene(sceneName),
                }).AddTo(this);

                var onSceneLoading = new ReactiveCommand<(string sceneName, float progress)>().AddTo(this);
                var onSceneLoadingDone = new ReactiveCommand<string>().AddTo(this);
                var onSceneUnloading = new ReactiveCommand<(string sceneName, float progress)>().AddTo(this);
                var onSceneUnloadingDone = new ReactiveCommand<string>().AddTo(this);
                _sceneLoadingEntity = new SceneLoading.SceneLoading.Entity(new SceneLoading.SceneLoading.Entity.Ctx
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
                    ("World", () => LoadWorld((World.World.WorldName.World_Test_0, Vector3.zero, Vector3.zero))),
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

            private async void LoadWorld((World.World.WorldName world, Vector3 worldPos, Vector3 worldRot) data)
            {
                await _loadingScreen.Show();
                await _gameLogic.UnloadAll();

                _ = await _gameLogic.GetWorld(data.world, new World.World.Ctx
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

        private IReactiveCommand<float> _onUpdate;

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            _onUpdate = new ReactiveCommand<float>().AddTo(this);
            _ = new Entity(new Entity.Ctx
            {
                OnUpdate = _onUpdate,
            }).AddTo(this);
        }

        private void Update()
        {
            _onUpdate.Execute(Time.deltaTime);
        }
    }
}

