using TMPro;
using UnityEngine;

namespace Dev.jeon.UI
{

    public class WaveUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _waveText;

        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _bossColor = Color.red;
       
        public void ShowWave(int waveIndex)
        {

            if (_waveText == null) return;
            _waveText.text = $"Wave {waveIndex}";
        }
        
    }

}