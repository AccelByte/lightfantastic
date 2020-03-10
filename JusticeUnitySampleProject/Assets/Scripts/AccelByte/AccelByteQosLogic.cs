using System.Collections.Generic;
using AccelByte.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AccelByteQosLogic : MonoBehaviour
{
    private static AccelByteQosLogic instance = new AccelByteQosLogic();
    public static AccelByteQosLogic Instance { get { return instance; } }
    private Qos qos;

    private static Dictionary<string, int> latencies = null;

    private void Start()
    {
        qos = AccelBytePlugin.GetQos();
        RefreshQosLatencies();
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            RefreshQosLatencies();
        };
    }

    public Dictionary<string, int> GetLatencies()
    {
        return latencies;
    }

    public void RefreshQosLatencies()
    {
        qos.GetServerLatencies(result =>
        {
            latencies = new Dictionary<string, int>(result.Value.Count);
            latencies = result.Value;
        });
    }
}
