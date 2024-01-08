using UnityEngine;
using USingleton.Utility;

public class ScreenEffect : MonoBehaviour
{
    private void OnDestroy()
    {
        // 등록된 동작을 제거합니다.
        Messager.RemoveMessage("FadeIn");
        Messager.RemoveMessage("FadeOut");
    }
}