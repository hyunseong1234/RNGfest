using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // "GameScene"은 이동할 실제 씬의 이름으로 변경하세요.
        SceneManager.LoadScene("Game");
    }
}
