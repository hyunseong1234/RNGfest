using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class SummonerBoss : BaseBoss
    {
        [Header("Summon Settings")]
        [SerializeField] private string _summonMobKey = "NormalEnemy"; // 풀링용 키값 (일반 몹)
        [SerializeField] private int _summonCount = 3;

        protected override void ApplySkillEffect()
        {
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            if (pool == null) return;

            for (int i = 0; i < _summonCount; i++)
            {
                var monster = pool.GetFromPool<Enemy>(_summonMobKey);
                if (monster != null)
                {
                    // 보스 주변에 겹치지 않게 무작위 좌표로 소환
                    Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                    monster.transform.position = this.transform.position + offset;

                    // TODO: 몬스터 초기화(Init) 함수가 있다면 여기서 호출해 주세요.
                }
            }

            Debug.Log($"[소환사 보스] 일반 몬스터 {_summonCount}마리 소환!");
        }
    }
}