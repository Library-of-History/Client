using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance;
    private readonly Queue<System.Action> _actions = new Queue<System.Action>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        lock (_actions)
        {
            while (_actions.Count > 0)
            {
                _actions.Dequeue().Invoke();
            }
        }
    }

    public static void RunOnMainThread(System.Action action)
    {
        lock (_instance._actions)
        {
            _instance._actions.Enqueue(action);
        }
    }
}