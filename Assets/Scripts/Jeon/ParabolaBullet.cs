using Dev.cheol.Manager;
using Dev.cheol.Model; // BaseObject나 Enemy가 있는 네임스페이스
using Dev.cheol.Stats;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Dev.jeon.Bullet
{

    public class ParabolaBullet : BaseObject
    {
        private Transform _target;
        private int _damage = 10;
        private Coroutine _moveCoroutine;

        [SerializeField] Vector3 targetPoint; //목표지점

        [Tooltip("올라가는 높이")]
        [SerializeField] private float height; //높이
        [Tooltip("떨어지는 속도")]
        [SerializeField] private float gravity; //중력
        [Tooltip("타겟없을때 사거리")]
        [SerializeField] private float range;
        [Tooltip("낙하지점")]
        [SerializeField] private float dropPoint; // 낙하지점
        [Tooltip("낙하포인트 높이")]
        [SerializeField] private float arriveHeight;
        //[Tooltip("공격 범위")]
        //[SerializeField] private Vector3 attackRange = new Vector3();

        public void Init(Transform target, int damage)
        {
            _target = target;
            _damage = damage;

            // 기존에 돌던 코루틴이 있다면 방어적으로 중지
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);

            if (_target != null)
            {
                _moveCoroutine = StartCoroutine(MoveToTarget());
            }
            else
            {
                ReturnToPool();
            }
        }

        private IEnumerator MoveToTarget()
        {
            // todo : 일단 머지 끝나고 작업 시작 이 주석제거하면서
            ////타겟없을때 사거리 계산해서 자동적으로 도착지점 정하는 로직인데 지금 사용 안해서 주석
            //if (target != Vector3.zero)
            //{
            //    targetPoint = target;
            //}
            //else
            //{
            //    targetPoint = AngleToDirection(transform.rotation.y, 5);
            //    Debug.Log("앵글 투 다이렉션 함수 작동 중");
            //}


            //Todo 영철_221103_변수명 번역 밑 주석작업 2211221 이후 할예정
            Vector3 start_pos = this.transform.position;
            Vector3 end_pos = targetPoint;
            Vector3 tXYZ;

            var dh = end_pos.y - start_pos.y;
            var mh = height - start_pos.y;

            tXYZ.y = Mathf.Sqrt(2 * gravity * mh);

            float a = gravity;
            float b = -2 * tXYZ.y;
            float c = 2 * dh;

            float dat = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
            tXYZ.x = -(start_pos.x - targetPoint.x) / dat;
            tXYZ.z = -(start_pos.z - targetPoint.z) / dat;

            float elapsed_time, posX, posY, posZ;
            elapsed_time = 0;
            Vector3 tpos;

            float durationRatio;

            float arriveTime = dat + dropPoint; //발사후 사라지는 시간


            while (elapsed_time <= arriveTime)
            {
                elapsed_time += Time.deltaTime;

                //_FillNum
                durationRatio = elapsed_time / arriveTime;

                posX = start_pos.x + tXYZ.x * elapsed_time;
                posY = start_pos.y + tXYZ.y * elapsed_time - 0.5f * gravity * elapsed_time * elapsed_time;
                posZ = start_pos.z + tXYZ.z * elapsed_time;

                tpos = new Vector3(posX, posY, posZ);

                this.transform.LookAt(tpos);
                this.transform.rotation *= Quaternion.Euler(90, 0, 0);
                this.transform.position = tpos;


                yield return null;
            }

            HitTarget();
            yield return null;
            ReturnToPool();
            //isCoroutine = false;
        }

        //Todo : 20221009_영철_유틸로 빼야되는데 일단보류
        /// <summary>
        /// 바라보는 각도의 앞 거리 계산 해주는 함수
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private Vector3 AngleToDirection(float angle, float range)
        {
            Vector3 pibot = transform.position;
            Vector3 direction = transform.forward;

            var quaternion = Quaternion.Euler(0, angle, 0);
            Vector3 newDirection = pibot + quaternion * direction * 10;

            return newDirection;
        }
        private void HitTarget()
        {
            // 타겟의 Enemy 컴포넌트나 BaseObject를 가져와 데미지 입힘
            var enemy = _target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnDamaged(_damage);
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            // 오브젝트 풀링 매니저를 통해 반납 (비활성화)
            // 직접 SetActive(false)를 해도 풀 매니저 구조에 따라 작동함
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }

        private void OnDisable()
        {
            // 오브젝트가 비활성화될 때 코루틴을 확실히 멈춰서 에러 방지
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
            _target = null;
        }
        public override void ObjectUpdate()
        {
            throw new System.NotImplementedException();
        }

    }
}

