using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainLobbyManager : MonoBehaviour
{
    [SerializeField] private ProfilePanel _porfilePanel;



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

    public void _OnClickLoadScene()
    {
        SceneManager.LoadScene(2); //
    }

}

