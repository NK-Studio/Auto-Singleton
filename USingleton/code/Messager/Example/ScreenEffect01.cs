using UnityEngine;
using USingleton.Utility;

public class ScreenEffect : MonoBehaviour
{
    private void Awake()
    {
        Messager.RegisterMessage("FadeIn", FadeIn);
        Messager.RegisterMessage("FadeOut", FadeOut);
    }
    
    public void FadeIn()
    {
        Debug.Log("FadeIn");
    }

    public void FadeOut()
    {
        Debug.Log("FadeOut");
    }
}
