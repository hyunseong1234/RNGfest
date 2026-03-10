using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectFramImage : MonoBehaviour
{

    [SerializeField] private TMP_Text _countText = null;
    [SerializeField] private TMP_Text _lvText = null;
    [SerializeField] private Image _filledImage = null;
    [SerializeField] private Image _towerImage = null;
    [SerializeField] private Image _arrowIcon = null;

    [SerializeField] private int _currentCount = 0;
    [SerializeField] private int _maxCount = 0;
    [SerializeField] private int _lv = 0;

}
