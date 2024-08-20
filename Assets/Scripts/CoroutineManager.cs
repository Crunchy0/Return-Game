using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager _instance = null;

    public static CoroutineManager getInstance()
    {
        if(_instance == null)
        {
            GameObject gObj = new GameObject("MasterMono");
            _instance = gObj.AddComponent<CoroutineManager>();
        }
        return _instance;
    }

    public void StartCustomCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}
