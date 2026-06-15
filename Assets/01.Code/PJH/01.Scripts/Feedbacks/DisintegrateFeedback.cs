using Code.Core.Feedbacks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

namespace Code.Code.Feedbacks
{
    public class DisintegrateFeedback : Feedback
    {
        [SerializeField] private float delayTime = 3f;
        [SerializeField] private VisualEffect feedbackEffect;
        [SerializeField] private SkinnedMeshRenderer meshRenderer;

        private bool _isAlreadyStart;
        private readonly int _dissolveHeight = Shader.PropertyToID("_DissolveHeight");

        public UnityEvent FeedbackComplete;
        
        public override void CreateFeedback()
        {
            if (_isAlreadyStart)
                return;

            _isAlreadyStart = true;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delayTime);
            seq.AppendCallback(() => feedbackEffect.Play());
            seq.Append(meshRenderer.material.DOFloat(-2f, _dissolveHeight, 0.5f));
            seq.AppendInterval(2f);
            seq.OnComplete(() => FeedbackComplete?.Invoke());
        }

        public override void StopFeedback()
        {
        }
    }
}