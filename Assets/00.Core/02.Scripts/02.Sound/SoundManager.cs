using System.Collections.Generic;
using Code.Core._02.Sound;
using Code.Core.GameEvent;
using GameEvents;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.RunTime;
using UnityEngine;

 public class SoundManager : MonoBehaviour
    {
        [SerializeField] private GameEventChannelSO soundChannel;
        [SerializeField] private PoolItemSO soundPlayer;
        
        [Inject] private PoolManagerMono _poolManagerMono;
        private SoundPlayer _bgmPlayer;

        private Dictionary<int, SoundPlayer>  _longSoundDict = new Dictionary<int, SoundPlayer>();
        private void Awake()
        {
            soundChannel.AddListener<PlaySFXEvent>(HandlePlaySFX);
            soundChannel.AddListener<PlayBGMEveent>(HandlePlayBGM);
            soundChannel.AddListener<StopBGMEvent>(HandleStopBGM);
            soundChannel.AddListener<PlayLongSFXEvent>(HandleLongSFX);
            soundChannel.AddListener<StopLongSFXEvent>(HandleStopLongSFX);
        }

        private void OnDestroy()
        {
            soundChannel.RemoveListener<PlaySFXEvent>(HandlePlaySFX);
            soundChannel.RemoveListener<PlayBGMEveent>(HandlePlayBGM);
            soundChannel.RemoveListener<StopBGMEvent>(HandleStopBGM);
            soundChannel.RemoveListener<PlayLongSFXEvent>(HandleLongSFX);
            soundChannel.RemoveListener<StopLongSFXEvent>(HandleStopLongSFX);
        }


        private void HandleLongSFX(PlayLongSFXEvent evt)
        {
            if (_longSoundDict.TryGetValue(evt.idxNumber, out SoundPlayer player))
            {
                player.StopAndReturnToPool();
                _longSoundDict.Remove(evt.idxNumber);   
            }

            SoundPlayer sPlayer = _poolManagerMono.Pop<SoundPlayer>(soundPlayer);
            sPlayer.transform.position = evt.position;
            sPlayer.PlaySound(evt.soundClip);
            _longSoundDict.Add(evt.idxNumber, sPlayer);
        }

        private void HandleStopLongSFX(StopLongSFXEvent obj)
        {
            if (_longSoundDict.TryGetValue(obj.idxNumber, out SoundPlayer player))
            {
                player.StopAndReturnToPool();
                _longSoundDict.Remove(obj.idxNumber);
            }
        }
        private void HandlePlaySFX(PlaySFXEvent evt)
        {
            SoundPlayer sfxPlayer = _poolManagerMono.Pop<SoundPlayer>(soundPlayer);
            sfxPlayer.transform.position = evt.position;
            sfxPlayer.PlaySound(evt.soundClip);
        }

        private void HandlePlayBGM(PlayBGMEveent evt)
        {
            _bgmPlayer = _poolManagerMono.Pop<SoundPlayer>(soundPlayer);
            _bgmPlayer.PlaySound(evt.bgmClip);
        }
        private void HandleStopBGM(StopBGMEvent evt)
        {
            _bgmPlayer?.StopAndReturnToPool();
        }
    }
    