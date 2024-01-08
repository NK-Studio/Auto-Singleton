using UnityEngine;
using USingleton.Utility;

public class ScreenEffect : MonoBehaviour
{
    private void Awake()
    {
        // 동작을 등록합니다.
        Messager.RegisterMessage("FadeIn", FadeIn);
        Messager.RegisterMessage("FadeOut", FadeOut);
    }

    /// <summary>
    /// 개체가 페이드 인됩니다.
    /// </summary>
    public void FadeIn()
    {
        Debug.Log("FadeIn");
    }

    /// <summary>
    /// 현재 개체를 페이드 아웃합니다.
    /// </summary>
    public void FadeOut()
    {
        Debug.Log("FadeOut");
    }

    private void OnDestroy()
    {
        // 등록된 동작을 제거합니다.
        Messager.RemoveMessage("FadeIn");
        Messager.RemoveMessage("FadeOut");
    }
}
