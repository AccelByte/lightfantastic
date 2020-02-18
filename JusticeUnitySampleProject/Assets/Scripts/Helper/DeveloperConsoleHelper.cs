using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperConsoleHelper : MonoBehaviour
{
    
    private static DeveloperConsoleHelper instance_;
    public static DeveloperConsoleHelper Instance
    {
        get
        {
            if (instance_ == null)
                CreateGameObject();
            return instance_;
        }
    }
    
    private void Awake()
    {
        if (instance_ != null)
        {
            Destroy(gameObject);
            return;
        }
        instance_ = this;
        DontDestroyOnLoad(gameObject);
    }

    private static void CreateGameObject()
    {
        if (instance_ == null)
        {
            new GameObject("Developer Console Helper").AddComponent<DeveloperConsoleHelper>();
        }
    }
    
    public void Refresh()
    {
        if (!LightFantasticConfig.DEVELOPER_CONSOLE_VISIBLE)
        {
            MainThreadTaskRunner.Instance.Run(()=>{StartCoroutine(ClearConsoleAsync());});
        }
    }

    IEnumerator ClearConsoleAsync()
    {
        while(!Debug.developerConsoleVisible)
        {
            yield return null;
        }
        yield return null;
        Debug.ClearDeveloperConsole();
    }
}
