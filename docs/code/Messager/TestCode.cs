using UnityEngine;
using USingleton.Utility;

public class TestCode : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Messager.Send("FadeIn");
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            Messager.Send("FadeOut");
        }
    }
}
