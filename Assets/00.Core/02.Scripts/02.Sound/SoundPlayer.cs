using GondrLib.ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.Audio;

namespace Code.Core._02.Sound
{
    public class SoundPlayer : MonoBehaviour, IPoolable
    {
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioSource audioSource;
        
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;
        private Pool _myPool;
        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            //do nothing
        }

        public async void PlaySound(SoundSO clipData)
        {
            if (clipData.audioType == SoundSO.AudioTypeS.SFX)
            {
                audioSource.outputAudioMixerGroup = sfxGroup;
            }

            else if (clipData.audioType == SoundSO.AudioTypeS.Music)
            {
                audioSource.outputAudioMixerGroup = musicGroup;
            }
            audioSource.volume = clipData.volume;
            audioSource.pitch = clipData.pitch;
            if (clipData.randomizePitch)
            {
                audioSource.pitch += Random.Range(-clipData.randomPitchModifier, clipData.randomPitchModifier);
            }
            
            audioSource.clip = clipData.clip;
            audioSource.loop = clipData.loop;

            if (clipData.loop == false)
            {
                audioSource.Play();
                await Awaitable.WaitForSecondsAsync(clipData.clip.length + 0.2f);
                _myPool.Push(this);
            }
        }

        public void StopAndReturnToPool()
        {
            audioSource.Stop();
            _myPool.Push(this);
        }
    }
}