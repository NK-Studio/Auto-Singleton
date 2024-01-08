using UnityEngine;
using USingleton;

public class TestCode : MonoBehaviour
{
    private void Start()
    {
        Singleton.Instance<GameManager>().HP = 100;
    }
}
