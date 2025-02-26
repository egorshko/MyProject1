using UnityEngine;
using UnityEngine.UI;

namespace Miner.UI 
{
    [ExecuteInEditMode]
    public class FactoryScreen : MonoBehaviour
    {
        [SerializeField] private int _padding = 10;
        [SerializeField] private ScrollRect _verticalScrollRect;

        private int _screenWidth;
        private int _screenHeight;

        private void Start()
        {
            UpdateScreen();
        }

        private void Update()
        {
            if (_screenWidth != Screen.width || _screenHeight != Screen.height) 
            {
                _screenWidth = Screen.width;
                _screenHeight = Screen.height;

                UpdateScreen();
            }

            if (Application.isPlaying && !Input.GetMouseButton(0)) 
            {
                var targetPos = _verticalScrollRect.content.localPosition;
                var nearestDistance = float.MaxValue;
                for (var i = 0; i < _verticalScrollRect.content.transform.childCount; i++)
                {
                    var childTransform = _verticalScrollRect.content.transform.GetChild(i);
                    var distance = Vector3.Distance(childTransform.position, _verticalScrollRect.viewport.transform.position);
                    if (distance >= nearestDistance) continue;
                    nearestDistance = distance;
                    targetPos.y = -childTransform.localPosition.y;
                }

                _verticalScrollRect.content.localPosition = Vector3.Lerp(_verticalScrollRect.content.localPosition, targetPos, Time.deltaTime * 25f);
            }
            
        }

        private void UpdateScreen()
        {
            var mainRectTransform = transform as RectTransform;

            var verticalGroup = GetComponentInChildren<VerticalLayoutGroup>(true);
            verticalGroup.padding.top = Mathf.RoundToInt(mainRectTransform.rect.width);
            verticalGroup.padding.right = _padding;
            verticalGroup.padding.bottom = Mathf.RoundToInt(mainRectTransform.rect.width);
            verticalGroup.padding.left = _padding;
            verticalGroup.spacing = _padding * 2;

            for (var i = 0; i < verticalGroup.transform.childCount; i++)
            {
                if (!verticalGroup.transform.GetChild(i).TryGetComponent<LayoutElement>(out var verticalLayoutElement)) continue;
                verticalLayoutElement.minHeight = mainRectTransform.rect.width - _padding * 2;
                var horizontalGroup = verticalLayoutElement.GetComponentInChildren<HorizontalLayoutGroup>(true);
                horizontalGroup.spacing = _padding * 2;
                for (var j = 0; j < horizontalGroup.transform.childCount; j++) 
                {
                    if (!horizontalGroup.transform.GetChild(j).TryGetComponent<LayoutElement>(out var horizontalLayoutElement)) continue;
                    horizontalLayoutElement.minWidth = mainRectTransform.rect.width - _padding * 2;
                }
            }
        }
    }
}

