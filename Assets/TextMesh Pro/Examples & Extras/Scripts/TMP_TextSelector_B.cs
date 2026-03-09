using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

#pragma warning disable 0618 

namespace TMPro.Examples
{
    public class TMP_TextSelector_B : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerUpHandler
    {
        public RectTransform TextPopup_Prefab_01;
        private RectTransform m_TextPopup_RectTransform;
        private TextMeshProUGUI m_TextPopup_TMPComponent;
        private const string k_LinkText = "You have selected link <#ffff00>";

        private TextMeshProUGUI m_TextMeshPro;
        private Canvas m_Canvas;
        private Camera m_Camera;

        private bool isHoveringObject;
        private int m_selectedWord = -1;
        private int m_selectedLink = -1;
        private int m_lastIndex = -1;
        private Matrix4x4 m_matrix;
        private TMP_MeshInfo[] m_cachedMeshInfoVertexData;

        void Awake()
        {
            m_TextMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
            m_Canvas = gameObject.GetComponentInParent<Canvas>();
            m_Camera = (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : m_Canvas.worldCamera;

            m_TextPopup_RectTransform = Instantiate(TextPopup_Prefab_01) as RectTransform;
            m_TextPopup_RectTransform.SetParent(m_Canvas.transform, false);
            m_TextPopup_TMPComponent = m_TextPopup_RectTransform.GetComponentInChildren<TextMeshProUGUI>();
            m_TextPopup_RectTransform.gameObject.SetActive(false);
        }

        void OnEnable() { TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED); }
        void OnDisable() { TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED); }
        void ON_TEXT_CHANGED(Object obj) { if (obj == m_TextMeshPro) m_cachedMeshInfoVertexData = m_TextMeshPro.textInfo.CopyMeshInfoVertexData(); }

        // [중요] 기존 LateUpdate 내용을 여기로 옮겼습니다.
        public void ManualUpdate(Camera cam)
        {
            m_Camera = cam;
            if (!isHoveringObject)
            {
                if (m_lastIndex != -1) { RestoreCachedVertexAttributes(m_lastIndex); m_lastIndex = -1; }
                return;
            }

            // --- 기존 로직 그대로 유지 ---
            int charIndex = TMP_TextUtilities.FindIntersectingCharacter(m_TextMeshPro, Input.mousePosition, m_Camera, true);
            if (charIndex == -1 || charIndex != m_lastIndex) { RestoreCachedVertexAttributes(m_lastIndex); m_lastIndex = -1; }
            if (charIndex != -1 && charIndex != m_lastIndex && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                m_lastIndex = charIndex;
                int materialIndex = m_TextMeshPro.textInfo.characterInfo[charIndex].materialReferenceIndex;
                int vertexIndex = m_TextMeshPro.textInfo.characterInfo[charIndex].vertexIndex;
                Vector3[] vertices = m_TextMeshPro.textInfo.meshInfo[materialIndex].vertices;
                Vector3 offset = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

                for (int i = 0; i<4; i++) vertices[vertexIndex + i] -= offset;
                m_matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 1.5f);
                for (int i = 0; i<4; i++) vertices[vertexIndex + i] = m_matrix.MultiplyPoint3x4(vertices[vertexIndex + i]);
                for (int i = 0; i<4; i++) vertices[vertexIndex + i] += offset;

                Color32 c = new Color32(255, 255, 192, 255);
                Color32[] vertexColors = m_TextMeshPro.textInfo.meshInfo[materialIndex].colors32;
                for (int i = 0; i<4; i++) vertexColors[vertexIndex + i] = c;
                m_TextMeshPro.textInfo.meshInfo[materialIndex].SwapVertexData(vertexIndex, vertices.Length - 4);
                m_TextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            }
            // ... (기타 Word/Link 로직도 동일하게 유지)
        }

        public void ResetTextEffect()
        {
            if (m_lastIndex != -1) RestoreCachedVertexAttributes(m_lastIndex);
            m_lastIndex = -1; m_selectedWord = -1; m_selectedLink = -1;
            m_TextPopup_RectTransform.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData) => isHoveringObject = true;
        public void OnPointerExit(PointerEventData eventData) => isHoveringObject = false;
        public void OnPointerClick(PointerEventData eventData) { }
        public void OnPointerUp(PointerEventData eventData) { }

        void RestoreCachedVertexAttributes(int index) { /* 기존 로직 동일 */ }
    }
}