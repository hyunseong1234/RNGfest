using Dev.cheol.Manager;
using Dev.jeon.Manager;
using System.Collections;
using UnityEngine;

namespace Dev.cheol.Model
{
    public abstract class BaseBullet : BaseObject
    {
        [Header("총알 공통 설정")]
        [SerializeField] protected float _speed = 20f;
        [SerializeField] protected float _damage = 10f;
        [SerializeField] protected FontColor _fontColor = FontColor.White;

        [Header("공통 리소스 (사운드/이펙트)")]
        [SerializeField] protected BaseObject _hitEffectPrefab;
        [SerializeField] protected AudioClip _fireSound;
        [SerializeField] protected AudioClip _hitSound;
        protected AudioSource _audioSource;

        protected Coroutine _moveCoroutine;
        protected SoundManager _sound; // 캐싱용

        protected override void Awake()
        {
            base.Awake();
            _sound = ServiceLocator.Instance.GetService<SoundManager>();
            _audioSource = GetComponent<AudioSource>();
        }

        public virtual void Init(Transform target, float damage, float speed = 20f)
        {
            _target = target;
            _damage = damage;
            _speed = speed;

            PlaySound(_fireSound);
            StartMove();
        }

        protected virtual void StartMove()
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveToTargetRoutine());
        }

        protected virtual IEnumerator MoveToTargetRoutine()
        {
            Vector3 lastPos = _target != null ? _target.position : transform.position;

            while (true)
            {
                if (_target != null && _target.gameObject.activeSelf) lastPos = _target.position;
                transform.position = Vector3.MoveTowards(transform.position, lastPos, _speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, lastPos) < 0.05f)
                {
                    OnHit(lastPos);
                    yield break;
                }
                yield return null;
            }
        }

        protected void OnHit(Vector3 hitPoint)
        {
            //PlaySound(_hitSound);
            //AudioSource.PlayClipAtPoint(_hitSound, hitPoint);
            SpawnHitEffect(hitPoint);
            ApplyHitLogic(hitPoint);
            ReturnToPool();
        }

        protected abstract void ApplyHitLogic(Vector3 hitPoint);

        protected void PlaySound(AudioClip clip)
        {
            if (clip == null || _sound == null) return;
            _audioSource.PlayOneShot(clip);
        }

        //  virtual 키워드 추가: 자식에서 재정의(override) 가능해짐!
        protected virtual void SpawnHitEffect(Vector3 pos)
        {
            if (_hitEffectPrefab == null) return;
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var effect = pool.GetFromPool<BaseObject>(_hitEffectPrefab);
            if (effect != null)
            {
                effect.transform.position = pos;
                effect.gameObject.SetActive(true);

                pool.StartCoroutine(ReturnHitEffectToPool(pool, effect, 1.0f));
            }
        }
        private IEnumerator ReturnHitEffectToPool(ObjectPoolingManger pool, BaseObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (effect != null && effect.gameObject.activeSelf)
            {
                effect.gameObject.SetActive(false);
                pool.ReturnPool(effect);
            }
        }
        protected virtual void ReturnToPool()
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _target = null;
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }

        public override void ObjectUpdate() { }
        protected virtual void OnDisable() { if (_moveCoroutine != null) StopCoroutine(_moveCoroutine); }
    }
}