using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game 
{
    public class GameLogic : BaseDisposable
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

        public GameLogic(Ctx ctx)
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
            foreach(var worldName in worlds)
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
}

