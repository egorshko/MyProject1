using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.City 
{
    public class CityBuildLogic : BaseDisposable
    {
        public enum BuildType : byte
        {
            Exit,
            Ships,
            Store,
            Bar,
        }

        [Serializable]
        public struct BuildData
        {
            [SerializeField] private BuildType _buildType;
            [SerializeField] private Transform _camPoint;

            public readonly BuildType BuildType => _buildType;
            public readonly Transform CamPoint => _camPoint;
        }

        [Serializable]
        public struct Data
        {
            [SerializeField] private Transform _rootCamPoint;
            [SerializeField] private Button _nextButton;
            [SerializeField] private Button _prevButton;
            [SerializeField] private Button _useButton;
            [SerializeField] private BuildData[] _builds;

            public readonly Transform RootCamPoint => _rootCamPoint;
            public readonly Button NextButton => _nextButton;
            public readonly Button PrevButton => _prevButton;
            public readonly Button UseButton => _useButton;
            public readonly BuildData[] Builds => _builds;
        }

        public struct Ctx
        {
            public IReactiveValue<Vector3> CurrentCameraPos;
            public IReactiveValue<Quaternion> CurrentCameraRot;
            public Action<BuildType> UseBuild;
            public Data Data;
        }

        private Ctx _ctx;

        public CityBuildLogic(Ctx ctx)
        {
            _ctx = ctx;

            var currentBuildIndex = 0;

            _ctx.Data.NextButton.onClick.RemoveAllListeners();
            _ctx.Data.NextButton.onClick.AddListener(() =>
            {
                currentBuildIndex++;
                if (currentBuildIndex >= _ctx.Data.Builds.Length) currentBuildIndex = 0;

                _ctx.CurrentCameraPos.Value = _ctx.Data.Builds[currentBuildIndex].CamPoint.position;
                _ctx.CurrentCameraRot.Value = _ctx.Data.Builds[currentBuildIndex].CamPoint.rotation;
            });

            _ctx.Data.PrevButton.onClick.RemoveAllListeners();
            _ctx.Data.PrevButton.onClick.AddListener(() =>
            {
                currentBuildIndex--;
                if (currentBuildIndex < 0) currentBuildIndex = _ctx.Data.Builds.Length - 1;

                _ctx.CurrentCameraPos.Value = _ctx.Data.Builds[currentBuildIndex].CamPoint.position;
                _ctx.CurrentCameraRot.Value = _ctx.Data.Builds[currentBuildIndex].CamPoint.rotation;
            });

            _ctx.Data.UseButton.onClick.RemoveAllListeners();
            _ctx.Data.UseButton.onClick.AddListener(() => 
            {
                _ctx.UseBuild.Invoke(_ctx.Data.Builds[currentBuildIndex].BuildType);
            });

            _ctx.CurrentCameraPos.Value = _ctx.Data.RootCamPoint.position;
            _ctx.CurrentCameraRot.Value = _ctx.Data.RootCamPoint.rotation;
            _ctx.Data.NextButton.gameObject.SetActive(false);
            _ctx.Data.PrevButton.gameObject.SetActive(false);
            _ctx.Data.UseButton.gameObject.SetActive(false);
            AsyncInit();
        }

        private async void AsyncInit() 
        {
            await UniTask.Delay(50, true);

            _ctx.CurrentCameraPos.Value = _ctx.Data.Builds[0].CamPoint.position;
            _ctx.CurrentCameraRot.Value = _ctx.Data.Builds[0].CamPoint.rotation;
            _ctx.Data.NextButton.gameObject.SetActive(true);
            _ctx.Data.PrevButton.gameObject.SetActive(true);
            _ctx.Data.UseButton.gameObject.SetActive(true);
        }
    }
}

