using Cysharp.Threading.Tasks;
using Game.City;
using Game.LoadingScreen;
using Game.MenuScreen;
using Game.Space;
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
            public Func<string, UniTask<SpaceEntryPoint>> GetSpace;
            public Func<string, UniTask<CityEntryPoint>> GetCity;
            public Func<string, UniTask<WorldEntryPoint>> GetWorld;
            public Func<string, UniTask> UnloadScene;
        }

        private const string SpaceName = "Space";
        private const string CityName = "City";

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
            new DisposeObserver(async () => await UnloadScene(loadingScreenName)).AddTo(this);
            return loadingScreenEntryPoint;
        }

        public async UniTask<MenuScreenEntryPoint> GetMenuScreen(MenuScreenEntryPoint.Ctx ctx)
        {
            const string menuScreenName = "MenuScreen";
            var menuScreenEntryPoint = await _ctx.GetMenuScreen.Invoke(menuScreenName);
            menuScreenEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(menuScreenName)).AddTo(this);
            return menuScreenEntryPoint;
        }

        public async UniTask<SpaceEntryPoint> GetSpace(SpaceEntryPoint.Ctx ctx)
        {
            var spaceEntryPoint = await _ctx.GetSpace.Invoke(SpaceName);
            spaceEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(SpaceName)).AddTo(this);
            return spaceEntryPoint;
        }

        private async UniTask UnloadSpace() => await UnloadScene(SpaceName);

        public async UniTask<CityEntryPoint> GetCity(CityEntryPoint.Ctx ctx)
        {
            var cityEntryPoint = await _ctx.GetCity.Invoke(CityName);
            cityEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(CityName)).AddTo(this);
            return cityEntryPoint;
        }

        private async UniTask UnloadCity() => await UnloadScene(CityName);

        public async UniTask<WorldEntryPoint> GetWorld(WorldEntryPoint.World world, WorldEntryPoint.Ctx ctx)
        {
            var worldEntryPoint = await _ctx.GetWorld.Invoke(world.ToString());
            worldEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(world.ToString())).AddTo(this);
            return worldEntryPoint;
        }

        private async UniTask UnloadWorlds() 
        {
            var worlds = Enum.GetNames(typeof(WorldEntryPoint.World));
            foreach(var worldName in worlds)
                await UnloadScene(worldName); 
        }

        private async UniTask UnloadScene(string sceneName) => await _ctx.UnloadScene(sceneName);

        public async UniTask UnloadAll()
        {
            await UnloadSpace();
            await UnloadCity();
            await UnloadWorlds();
        }
    }
}

