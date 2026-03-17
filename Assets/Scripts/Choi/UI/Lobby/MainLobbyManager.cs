using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainLobbyManager : MonoBehaviour
{
    [SerializeField] private ProfilePanel _porfilePanel;

    public ProfilePanel PorfilePanel { get => _porfilePanel; set => _porfilePanel = value; }

    private void OnEnable()
    {
        var playfab = PlayFabDataManager.Instance;
        if (UIFind.TryGetOrFindUI(ref _porfilePanel))
        {
            string nickName = playfab.userData._userNickName.ToString();
            string gold = playfab.userData._gold.ToString();
            string juwel = playfab.userData._jewel.ToString();
            _porfilePanel.SetProfile(nickName, gold, juwel);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var _userData = PlayFabDataManager.Instance.userData;
            _userData._gold += 1000;
            PorfilePanel.SetProfile(_userData._userNickName.ToString(), _userData._gold.ToString(), _userData._jewel.ToString());
        }
    }

    public void _OnClickLoadScene()
    {
        SceneManager.LoadScene(2); //
    }

}

