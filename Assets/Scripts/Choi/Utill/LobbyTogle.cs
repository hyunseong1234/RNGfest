using UnityEngine;
using UnityEngine.UI;

public class LobbyTogle : MonoBehaviour
{
    [SerializeField] private Image _lobbyBtn;
    [SerializeField] private Image _teamBtn;

    private Color _activeColor = new Color(1f, 0.82f, 0.2f); // 노란색 (#FFD232)
    private Color _inactiveColor = new Color(0.7f, 0.7f, 0.7f); // 회색 (#B4B4B4)

    private void OnEnable()
    {
        if (UIFind.TryGetOrFindUI(ref _lobbyBtn, "Lobby(Button)"))
        {
            if (_lobbyBtn.TryGetComponent<Toggle>(out Toggle toggle))
            {
                toggle.isOn = true;
            }
        }
        OnChangeTogle(true);
    }
    /// <summary>
    /// 토글 상태를 변경합니다. 
    /// true면 로비 활성화, false면 팀 화면 활성화
    /// </summary>
    public void OnChangeTogle(bool isLobby)
    {
        if (UIFind.TryGetOrFindUI(ref _lobbyBtn, "Lobby(Button)")) _lobbyBtn.color = isLobby ? _activeColor : _inactiveColor;
        if (UIFind.TryGetOrFindUI(ref _teamBtn, "Team(Button)")) _teamBtn.color = isLobby ? _inactiveColor : _activeColor;
    }
}