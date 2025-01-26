using Game.City;
using Game.LoadingScreen;
using Game.MenuScreen;
using Game.Space;
using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Game 
{
    public class GameLogic : BaseDisposable
    {
        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;

            public Func<string, Task<LoadingScreenEntryPoint>> GetLoadingScreen;
            public Func<string, Task<MenuScreenEntryPoint>> GetMenuScreen;
            public Func<string, Task<SpaceEntryPoint>> GetSpace;
            public Func<string, Task<CityEntryPoint>> GetCity;
            public Func<string, Task> UnloadScene;
        }

        private const string SpaceName = "Space";
        private const string CityName = "City";

        private readonly Ctx _ctx;

        public GameLogic(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async Task<LoadingScreenEntryPoint> GetLoadingScreen(LoadingScreenEntryPoint.Ctx ctx)
        {
            const string loadingScreenName = "LoadingScreen";
            var loadingScreenEntryPoint = await _ctx.GetLoadingScreen.Invoke(loadingScreenName);
            loadingScreenEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(loadingScreenName)).AddTo(this);
            return loadingScreenEntryPoint;
        }

        public async Task<MenuScreenEntryPoint> GetMenuScreen(MenuScreenEntryPoint.Ctx ctx)
        {
            const string menuScreenName = "MenuScreen";
            var menuScreenEntryPoint = await _ctx.GetMenuScreen.Invoke(menuScreenName);
            menuScreenEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(menuScreenName)).AddTo(this);
            return menuScreenEntryPoint;
        }

        public async Task<SpaceEntryPoint> GetSpace(SpaceEntryPoint.Ctx ctx)
        {
            var spaceEntryPoint = await _ctx.GetSpace.Invoke(SpaceName);
            spaceEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(SpaceName)).AddTo(this);
            return spaceEntryPoint;
        }

        private async Task UnloadSpace() => await UnloadScene(SpaceName);

        public async Task<CityEntryPoint> GetCity(CityEntryPoint.Ctx ctx)
        {
            var cityEntryPoint = await _ctx.GetCity.Invoke(CityName);
            cityEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(CityName)).AddTo(this);
            return cityEntryPoint;
        }

        private async Task UnloadCity() => await UnloadScene(CityName);

        private async Task UnloadScene(string sceneName) => await _ctx.UnloadScene(sceneName);

        public async Task UnloadAll()
        {
            await UnloadSpace();
            await UnloadCity();
        }
    }
}

