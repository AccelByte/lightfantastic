using System.Collections.Generic;
using AccelByte.Api;
using UnityEngine;

public class AccelByteQosLogic : MonoBehaviour
{
    private static AccelByteQosLogic instance = new AccelByteQosLogic();
    public static AccelByteQosLogic Instance { get { return instance; } }
    private Qos qos;

    private static Dictionary<string, int> latencies = null;

    private void Start()
    {
        qos = AccelBytePlugin.GetQos();
        qos.GetServerLatencies(result =>
        {
            latencies = new Dictionary<string, int>(result.Value.Count);
            latencies = result.Value;
        });
    }

    public Dictionary<string, int> GetLatencies()
    {
        return latencies;
    }
}
