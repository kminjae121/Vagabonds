using Code.Core.Feedbacks;
using DG.Tweening;
using UnityEngine;

namespace Code.Code.Feedbacks
{
    public class BlinkFeedback : Feedback
    {
        [SerializeField] private SkinnedMeshRenderer meshRenderer;
        [SerializeField] private float blinkDuration = 0.15f;
        [SerializeField] private float blinkIntensity = 0.25f;

        private readonly int _blinkHash = Shader.PropertyToID("_BlinkValue");
        private Material _material;
        private Tweener _blinkTween;

        private void Awake()
        {
            _material = meshRenderer.material;
        }

        public override void CreateFeedback()
        {
            _blinkTween?.Kill();

            _blinkTween = _material
                .DOFloat(blinkIntensity, _blinkHash, blinkDuration * 0.5f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }

        public override void StopFeedback()
        {
            _blinkTween?.Kill();
            _material.SetFloat(_blinkHash, 0f);
        }
    }
}