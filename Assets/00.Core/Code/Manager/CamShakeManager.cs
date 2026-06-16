using Code.Core.Events.Bus;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.Cam
{
    public class CamShakeManager : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource source;
        [SerializeField] private float _minimumForce = 0.01f;

        private bool _reportedMissingSource;

        private void Awake()
        {
            if (source == null)
                source = GetComponent<CinemachineImpulseSource>();
        }

        private void OnEnable()
        {
            Bus<CamShakeEvent>.Subscribe(ShakeCam);
        }

        private void OnDisable()
        {
            Bus<CamShakeEvent>.Unsubscribe(ShakeCam);
        }

        private void ShakeCam(CamShakeEvent evt)
        {
            if (Mathf.Abs(evt.force) < _minimumForce)
                return;

            if (source == null)
            {
                if (!_reportedMissingSource)
                {
                    Debug.LogWarning($"{nameof(CamShakeManager)} requires a CinemachineImpulseSource.", this);
                    _reportedMissingSource = true;
                }

                return;
            }

            source.GenerateImpulse(evt.force);
        }
    }
}
