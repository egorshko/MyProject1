using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.MenuScreen 
{
    public class MenuScreenLogic : BaseDisposable
    {
        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public MenuScreenEntryPoint.Data Data;
            public IReactiveValue<bool> IsShowMenu;
        }

        private Ctx _ctx;

        private Stack<GameObject> _currentButtons = new ();

        public MenuScreenLogic(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.Data.DefaultButton.gameObject.SetActive(false);
        }

        public void SetButtons(MenuScreenType screenType, string header, Action onBackgroundClick, params (string header, Action onClick)[] buttons)
        {
            while (_currentButtons.Count > 0)
                UnityEngine.Object.Destroy(_currentButtons.Pop());

            switch (screenType)
            {
                case MenuScreenType.Horizontal:
                    _ctx.Data.LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    break;
                case MenuScreenType.Vertical:
                    _ctx.Data.LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    break;
            }

            _ctx.Data.Header.text = header;

            _ctx.Data.Group.blocksRaycasts = true;
            _ctx.Data.Group.alpha = 1f;

            _ctx.Data.BackgroundButton.onClick.RemoveAllListeners();
            _ctx.Data.BackgroundButton.onClick.AddListener(() => onBackgroundClick?.Invoke());

            foreach (var button in buttons) 
            {
                var currentButton = UnityEngine.Object.Instantiate(_ctx.Data.DefaultButton, _ctx.Data.DefaultButton.transform.parent);

                var currentButtonTextArea = currentButton.GetComponentInChildren<TMP_Text>(true);
                currentButtonTextArea.text = button.header;

                currentButton.onClick.RemoveAllListeners();
                currentButton.onClick.AddListener(() => button.onClick?.Invoke());

                currentButton.gameObject.SetActive(true);

                _currentButtons.Push(currentButton.gameObject);
            }

            _ctx.IsShowMenu.Value = true;

            Time.timeScale = 0f;
        }

        public void Hide() 
        {
            while (_currentButtons.Count > 0)
                UnityEngine.Object.Destroy(_currentButtons.Pop());

            _ctx.Data.Group.blocksRaycasts = false;
            _ctx.Data.Group.alpha = 0f;

            _ctx.IsShowMenu.Value = false;

            Time.timeScale = 1f;
        }
    }
}

