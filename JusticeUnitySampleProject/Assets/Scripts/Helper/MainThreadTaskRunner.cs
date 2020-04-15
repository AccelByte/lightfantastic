// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using System;
using System.Collections.Generic;

public class MainThreadTaskRunner : MonoBehaviour
{

    public enum UpdateType
    {
        FixedUpdate,
        Update,
        LateUpdate
    }

    private static MainThreadTaskRunner instance_;
    public static MainThreadTaskRunner Instance
    {
        get
        {
            if (instance_ == null)
                CreateGameObject();
            return instance_;
        }
    }

    private Dictionary<UpdateType, Queue<Action>> tasks = new Dictionary<UpdateType, Queue<Action>>();

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

    /// <summary>
    /// Create The task runner persistent gameobject
    /// </summary>
    /// <param></param>
    public static void CreateGameObject()
    {
        if (instance_ != null)
            return;
        new GameObject("Main Thread Task Runner").AddComponent<MainThreadTaskRunner>();
    }

    /// <summary>
    /// Enqueue a task to be run on the main thread
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <param name="type">Where you want the task to run</param>
    public void Run(Action task, UpdateType type = UpdateType.Update)
    {
        if (!tasks.ContainsKey(type))
        {
            tasks.Add(type, new Queue<Action>());
        }
        Queue<Action> taskQueue = tasks[type];
        lock (taskQueue)
        {
            taskQueue.Enqueue(task);
        }
    }

    private void ExecuteTasks(UpdateType type)
    {
        if (tasks.ContainsKey(type))
        {
            Queue<Action> taskQueue = tasks[type];
            while (taskQueue.Count > 0)
                taskQueue.Dequeue()();
        }
    }

    private void LateUpdate()
    {
        ExecuteTasks(UpdateType.LateUpdate);
    }

    private void Update()
    {
        ExecuteTasks(UpdateType.Update);
    }

    private void FixedUpdate()
    {
        ExecuteTasks(UpdateType.FixedUpdate);
    }

    /// <summary>
    /// If you want to destroy the instance for some reason
    /// </summary>
    /// <param></param>
    public void DestroyGameObject()
    {
        instance_ = null;
        Destroy(gameObject);
    }
}
