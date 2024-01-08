using USingleton.SelfSingleton;

public class GameManager : Singleton
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
