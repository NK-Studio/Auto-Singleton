using USingleton.SelfSingleton;

public class GameManager : Singleton
{
    protected override bool DontDestroyOnLoad()
    {
        return false;
    }
}
