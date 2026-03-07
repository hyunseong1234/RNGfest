using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Stats
{
    [Serializable]
    public class Stat
    {
        [SerializeField] private float _baseValue; // 인스펙터에서 수정할 기본값
        private float _lastValue;
        private bool _isDirty = true; // 값이 변했는지 확인용

        // 수정을 가하는 버프/아이템 리스트
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();

        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    _lastValue = CalculateFinalValue();
                    _isDirty = false;
                }
                return _lastValue;
            }
        }

        public float BaseValue
        {
            get => _baseValue;
            set
            {
                _baseValue = value;
                _isDirty = true; // 값이 설정되면 새로 계산하도록 마킹
            }
        }

        // 수치 추가 (예: 슬로우 걸기)
        public void AddModifier(StatModifier mod)
        {
            _modifiers.Add(mod);
            _isDirty = true;
        }

        // 수치 제거 (예: 버프 종료)
        public void RemoveModifier(StatModifier mod)
        {
            _modifiers.Remove(mod);
            _isDirty = true;
        }

        // 특정 소스가 준 모든 수치 제거
        public void RemoveAllModifiersFromSource(object source)
        {
            _modifiers.RemoveAll(m => m.Source == source);
            _isDirty = true;
        }

        // 모든 수치 초기화
        public void ClearModifiers()
        {
            _modifiers.Clear();
            _isDirty = true;
        }

        // 실제 계산 로직
        private float CalculateFinalValue()
        {
            float finalValue = _baseValue;

            // 1. 고정치(Flat) 먼저 다 더함
            float flatSum = 0;
            // 2. 퍼센트(Percent)는 곱함
            float percentMult = 1;

            foreach (var mod in _modifiers)
            {
                if (mod.Type == StatModType.Flat) flatSum += mod.Value;
                else if (mod.Type == StatModType.Percent) percentMult *= (1 + mod.Value);
            }

            return (finalValue + flatSum) * percentMult;
        }
    }
}