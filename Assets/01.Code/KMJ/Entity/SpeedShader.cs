using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Code.KMJ.Entity
{
    public class SpeedShader : MonoBehaviour
    {
        [SerializeField] private Image img;
        private Material _mat;
        
        private const string MASK_PROPERTY_NAME = "_Mask_Size";
        private int _maskSizeID;

        private Tween _maskTween;
        
        private void Awake()
        {
            _mat = img.material;
            _maskSizeID = Shader.PropertyToID(MASK_PROPERTY_NAME);
            SetMaskSize(1.2f);  
        }


        public void SetMaskSize(float value)
        {
            _maskTween?.Kill();

            _maskTween = _mat.DOFloat(value, _maskSizeID, 0.7f)
                .SetEase(Ease.OutQuart);
        }
    }
}