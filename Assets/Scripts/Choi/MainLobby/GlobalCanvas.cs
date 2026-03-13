using UnityEngine;

public class GlobalCanvas : MonoBehaviour
{
    public static GlobalCanvas Instance;
    [SerializeField] private LoadingImage _loadingPanel;

    public LoadingImage LoadingPanel { get => _loadingPanel; set => _loadingPanel = value; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 
        }
        else
        {
            Destroy(gameObject);
        }
    }
}