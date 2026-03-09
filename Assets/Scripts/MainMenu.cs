using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject content;
    public float slideDuration = 0.4f;
    public float screenWidth = 1920f;

    public void MoveToLobby()
    {
        StartCoroutine(Slide(0));
    }

    public void MoveToTeam()
    {
        StartCoroutine(Slide(-screenWidth));
    }

    IEnumerator Slide(float targetX)
    {
        Vector3 startPos = content.transform.localPosition;
        Vector3 endPos = new Vector3(targetX, 0, 0);
        float elapsed = 0;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = t * t * (3f - 2f * t);

            content.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        content.transform.localPosition = endPos;
    }
}