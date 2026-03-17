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

    /// <summary>
    /// 한개전용
    /// </summary>
    /// <param name="id"></param>
    public void SetProfile(string data, string value) // 값을 같이 받아야 합니다
    {
        if (data == "id")
        {
            _idText.text = value; // id 텍스트에 id 값을 넣음
        }
        else if (data == "gold")
        {
            _goldText.text = value; // gold 텍스트에 gold 값을 넣음
        }
        else if (data == "juwel")
        {
            _juwelText.text = value;
        }
    }

}
