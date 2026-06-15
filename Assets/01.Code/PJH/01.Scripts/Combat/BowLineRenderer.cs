using UnityEngine;
using DG.Tweening; 

namespace Code.Combat
{
    public class BowLineRenderer : MonoBehaviour
    {
        [SerializeField] private LineRenderer bowstringLineRenderer;
        [SerializeField] private Transform bowstringTop;
        [SerializeField] private Transform bowstringMiddle;
        [SerializeField] private Transform bowstringBottom;

        [Header("Bow Draw Settings")]
        [SerializeField] private Transform drawTargetPoint; 
        
        private Vector3 _originalMiddleLocalPos;
        private Tween _drawTween;

        private void Awake()
        {
            if (bowstringMiddle != null)
                _originalMiddleLocalPos = bowstringMiddle.localPosition;
        }

        private void LateUpdate()
        {
            CreateBowstring();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(bowstringLineRenderer != null)
                CreateBowstring();
        }
#endif 
        
        public void LoadBowstring(float duration)
        {
            _drawTween?.Kill();

            if (drawTargetPoint != null && bowstringMiddle.parent != null)
            {
                Vector3 targetLocalPos = bowstringMiddle.parent.InverseTransformPoint(drawTargetPoint.position);
                _drawTween = bowstringMiddle.DOLocalMove(targetLocalPos, duration)
                                            .SetEase(Ease.OutQuad);
            }
        }
        
        public void ReleaseBowstring(float duration)
        {
            _drawTween?.Kill();
            
            _drawTween = bowstringMiddle.DOLocalMove(_originalMiddleLocalPos, duration)
                                        .SetEase(Ease.OutBack);
        }

        private void CreateBowstring()
        {
            if (!bowstringLineRenderer || !bowstringTop || !bowstringMiddle || !bowstringBottom)
                return;
            
            bowstringLineRenderer.positionCount = 3;
            bowstringLineRenderer.SetPosition(0, bowstringTop.position);
            bowstringLineRenderer.SetPosition(1, bowstringMiddle.position);
            bowstringLineRenderer.SetPosition(2, bowstringBottom.position);
        }

        private void OnDestroy()
        {
            _drawTween?.Kill();
        }
    }
}