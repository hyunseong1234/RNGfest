using TMPro;
using UnityEngine;

public class ProfilePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _idText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _juwelText;

    /// <summary>
    /// 아이디 골드 쥬얼 순이다.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="gold"></param>
    /// <param name="juwel"></param>
    public void SetProfile(string id, string gold, string juwel)
    {
        _idText.text = id;
        _goldText.text = gold;
        _juwelText.text = juwel;
    }
}
