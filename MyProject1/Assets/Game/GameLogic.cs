using Cysharp.Threading.Tasks;
using Game.LoadingScreen;
using Game.MenuScreen;
using Game.World;
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

            public Func<string, UniTask<LoadingScreenEntryPoint>> GetLoadingScreen;
            public Func<string, UniTask<MenuScreenEntryPoint>> GetMenuScreen;
            public Func<string, UniTask<WorldEntryPoint>> GetWorld;
            public Func<string, UniTask> UnloadScene;
        }

        private readonly Ctx _ctx;

        public GameLogic(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask<LoadingScreenEntryPoint> GetLoadingScreen(LoadingScreenEntryPoint.Ctx ctx)
        {
            const string loadingScreenName = "LoadingScreen";
            var loadingScreenEntryPoint = await _ctx.GetLoadingScreen.Invoke(loadingScreenName);
            loadingScreenEntryPoint.Init(ctx);
            new DisposeObserver(async () => await _ctx.UnloadScene(loadingScreenName)).AddTo(this);
            return loadingScreenEntryPoint;
        }

        public async UniTask<MenuScreenEntryPoint> GetMenuScreen(MenuScreenEntryPoint.Ctx ctx)
        {
            const string menuScreenName = "MenuScreen";
            var menuScreenEntryPoint = await _ctx.GetMenuScreen.Invoke(menuScreenName);
            menuScreenEntryPoint.Init(ctx);
            new DisposeObserver(async () => await _ctx.UnloadScene(menuScreenName)).AddTo(this);
            return menuScreenEntryPoint;
        }

        public async UniTask<WorldEntryPoint> GetWorld(WorldEntryPoint.World world, WorldEntryPoint.Ctx ctx)
        {
            var worldEntryPoint = await _ctx.GetWorld.Invoke(world.ToString());
            worldEntryPoint.Init(ctx);
            return worldEntryPoint;
        }

        private async UniTask UnloadWorlds() 
        {
            var worlds = Enum.GetNames(typeof(WorldEntryPoint.World));
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

