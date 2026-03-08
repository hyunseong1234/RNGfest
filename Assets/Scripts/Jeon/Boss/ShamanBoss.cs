using Dev.cheol.Manager;
using Dev.jeon.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class ShamanBoss : BaseBoss
    {
        protected override void ApplySkillEffect()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var towers = main.SpawnTowers;

            if (towers == null || towers.Count == 0) return;

            // 1. 타워 리스트를 복사해서 랜덤하게 섞음 (Linq 활용)
            var randomTowers = towers.OrderBy(x => Random.value).ToList();

            // 2. 타워 개수의 절반 계산 (최소 1개)
            int targetCount = Mathf.Max(1, randomTowers.Count / 2);

            // 3. 절반만큼만 등급 하락 실행
            for (int i = 0; i < targetCount; i++)
            {
                randomTowers[i].Downgrade();
                // 시각적 연출: 타워 위치에 보라색 저주 이펙트 뙇!
            }

            Debug.Log($"주술사 보스: 타워 {targetCount}개의 등급을 강제로 낮췄습니다!");
        }
    }

}
