using UnityEngine;
using UnityEngine.Audio;
using Dev.cheol.Manager;
using System.Collections;

namespace Dev.cheol.Manager
{
    public class SoundManager : BaseManager
    {
        [Header("오디오 리소스")]
        [SerializeField] private AudioClip _bgmClip;
        [SerializeField] private AudioMixer _masterMixer;

        [Header("오디오 설정")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private string _sfxPoolTag = "SoundObject";

        private void Awake()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.loop = true;
                _bgmSource.playOnAwake = false;
            }
        }

        private void Start()
        {
            if (_bgmClip != null) PlayBGM(_bgmClip);
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || _bgmSource.clip == clip) return;
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;

            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            SoundObject soundObj = pool.GetFromPool<SoundObject>(_sfxPoolTag);

            if (soundObj != null)
            {
                soundObj.Source.PlayOneShot(clip);
                StartCoroutine(ReturnToPoolAfterPlay(soundObj, clip.length));
            }
        }

        private IEnumerator ReturnToPoolAfterPlay(SoundObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            obj.OnDespawn();
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(obj);
        }

        // 설정창 열고 닫을 때 호출
        public void SetPause(bool isPause)
        {
            if (isPause)
            {
                _bgmSource.Pause();
                // 믹서 그룹 이름을 확인하여 설정 (보통 "SFX" 파라미터 노출 필요)
                _masterMixer.SetFloat("SFX_Volume", -80f);
            }
            else
            {
                _bgmSource.UnPause();
                _masterMixer.SetFloat("SFX_Volume", 0f);
            }
        }

        public override void HandleEvent(string data) { }
    }
}