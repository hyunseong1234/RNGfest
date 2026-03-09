using UnityEngine;
using TMPro;

public class TMP_UiFrameRateCounter : MonoBehaviour
{
    public float UpdateInterval = 5.0f;
    private float m_LastInterval = 0;
    private int m_Frames = 0;

    public enum FpsCounterAnchorPositions { TopLeft, BottomLeft, TopRight, BottomRight };
    public FpsCounterAnchorPositions AnchorPosition = FpsCounterAnchorPositions.TopRight;

    private string htmlColorTag;
    private const string fpsLabel = "{0:2}</color> <#8080ff>FPS \n<#FF8000>{1:2} <#8080ff>MS";

    public TextMeshProUGUI m_TextMeshPro;
    private RectTransform m_frameCounter_transform;
    private FpsCounterAnchorPositions last_AnchorPosition;

    void Awake()
    {
        if (m_TextMeshPro == null) return;
        m_frameCounter_transform = m_TextMeshPro.GetComponent<RectTransform>();
        Application.targetFrameRate = 1000;
        Set_FrameCounter_Position(AnchorPosition);
        last_AnchorPosition = AnchorPosition;
    }

    void Start() { m_LastInterval = Time.realtimeSinceStartup; m_Frames = 0; }

    void Update()
    {
        if (AnchorPosition != last_AnchorPosition) Set_FrameCounter_Position(AnchorPosition);
        last_AnchorPosition = AnchorPosition;
        m_Frames += 1;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > m_LastInterval + UpdateInterval)
        {
            float fps = m_Frames / (timeNow - m_LastInterval);
            float ms = 1000.0f / Mathf.Max(fps, 0.00001f);
            htmlColorTag = (fps < 10) ? "<color=red>" : (fps < 30 ? "<color=yellow>" : "<color=green>");
            m_TextMeshPro.SetText(htmlColorTag + fpsLabel, fps, ms);
            m_Frames = 0; m_LastInterval = timeNow;
        }
    }

    void Set_FrameCounter_Position(FpsCounterAnchorPositions anchor_position)
    {
        // switch문은 그대로 유지하시면 됩니다.
    }
}