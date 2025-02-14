using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Miner
{
    internal sealed class Miner : BaseDisposableMB
    {
        [Serializable]
        private struct Data
        {
            [SerializeField] private LoadingScreen.LoadingScreen.Data _loadingScreenData;
        }

        [SerializeField] private Data _data;

        private IReactiveCommand<float> _onUpdate;

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            _onUpdate = new ReactiveCommand<float>().AddTo(this);
        }

        private void Update()
        {
            _onUpdate.Execute(Time.deltaTime);
        }
    }
}

