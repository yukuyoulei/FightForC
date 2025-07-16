// 2025/7/11 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public static class EventRecorder
{
    private static List<RecordedEvent> recordedEvents = new List<RecordedEvent>();
    private static float startTime;
    private static bool isRecording = false;
    private static Dictionary<Button, UnityEngine.Events.UnityAction> buttonListeners = new Dictionary<Button, UnityEngine.Events.UnityAction>();
    
    // 数据持久化路径
    private static string DataPath => System.IO.Path.Combine(Application.temporaryCachePath, "AutoTestEvents.json");
    
    // 静态构造函数，在类首次使用时加载数据
    static EventRecorder()
    {
        LoadRecordedEvents();
    }

    public static void StartRecording()
    {
        recordedEvents.Clear();
        startTime = Time.realtimeSinceStartup;
        isRecording = true;

        // 为所有现有的 Button 添加监听器
        AddListenersToAllButtons();

        // 监听场景变化，以便为新创建的按钮添加监听器
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        
        // 监听Scene视图的鼠标事件
        SceneView.duringSceneGui += OnSceneGUI;
        
        // 监听Game视图的事件（通过EditorApplication.update检查输入）
        EditorApplication.update += CheckGameViewInput;

        Debug.Log("Recording started.");
    }

    public static void StopRecording()
    {
        isRecording = false;
        
        // 移除我们添加的监听器，但保留原有的监听器
        RemoveOurListeners();
        
        // 停止监听场景变化和Scene视图事件
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= CheckGameViewInput;

        // 保存录制的事件到文件
        SaveRecordedEvents();

        Debug.Log($"Recording stopped. {recordedEvents.Count} events recorded and saved.");
    }

    private static void AddListenersToAllButtons()
    {
        var buttons = GameObject.FindObjectsOfType<Button>();
        foreach (var button in buttons)
        {
            if (!buttonListeners.ContainsKey(button))
            {
                UnityEngine.Events.UnityAction listener = () => RecordButtonClick(button);
                button.onClick.AddListener(listener);
                buttonListeners[button] = listener;
            }
        }
    }

    private static void RemoveOurListeners()
    {
        foreach (var kvp in buttonListeners)
        {
            if (kvp.Key != null)
            {
                kvp.Key.onClick.RemoveListener(kvp.Value);
            }
        }
        buttonListeners.Clear();
    }

    private static void OnHierarchyChanged()
    {
        if (isRecording)
        {
            // 为新创建的按钮添加监听器
            AddListenersToAllButtons();
        }
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!isRecording) return;

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0) // 左键点击
        {
            // 记录Scene视图中的点击事件
            Vector2 mousePosition = e.mousePosition;
            GameObject hitObject = HandleUtility.PickGameObject(mousePosition, false);
            
            if (hitObject != null)
            {
                recordedEvents.Add(new RecordedEvent
                {
                    Time = Time.realtimeSinceStartup - startTime,
                    ClickedObjectName = hitObject.name,
                    IsUIButton = false,
                    MousePosition = mousePosition,
                    EventType = EventType.MouseDown
                });

                Debug.Log($"Recorded Scene Click: {hitObject.name} at {mousePosition}");
            }
        }
    }

    private static void CheckGameViewInput()
    {
        if (!isRecording || !Application.isPlaying) return;

        // 检查鼠标点击事件
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            
            // 使用射线检测来找到点击的对象
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Ray ray = mainCamera.ScreenPointToRay(mousePos);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    recordedEvents.Add(new RecordedEvent
                    {
                        Time = Time.realtimeSinceStartup - startTime,
                        ClickedObjectName = hit.collider.gameObject.name,
                        IsUIButton = false,
                        MousePosition = mousePos,
                        EventType = EventType.MouseDown
                    });

                    Debug.Log($"Recorded Game View Click: {hit.collider.gameObject.name} at {mousePos}");
                }
            }
        }

        // 检查键盘输入
        if (Input.inputString.Length > 0)
        {
            foreach (char c in Input.inputString)
            {
                if (c != '\b' && c != '\n' && c != '\r') // 忽略退格、换行等特殊字符
                {
                    recordedEvents.Add(new RecordedEvent
                    {
                        Time = Time.realtimeSinceStartup - startTime,
                        ClickedObjectName = $"KeyInput_{c}",
                        IsUIButton = false,
                        MousePosition = Vector2.zero,
                        EventType = EventType.KeyDown
                    });

                    Debug.Log($"Recorded Key Input: {c}");
                }
            }
        }
    }

    public static void StartReplay(Action onReplayComplete)
    {
        Debug.Log($"StartReplay called. Recorded events count: {recordedEvents.Count}");
        
        if (recordedEvents.Count == 0)
        {
            Debug.LogWarning("No recorded events to replay. Please record events first.");
            onReplayComplete?.Invoke();
            return;
        }

        // 确保之前的回放已经停止
        EditorApplication.update -= ReplayEvents;
        
        EditorApplication.update += ReplayEvents;
        replayIndex = 0;
        replayStartTime = Time.realtimeSinceStartup;
        replayCompleteCallback = onReplayComplete;
        
        Debug.Log($"Replay started with {recordedEvents.Count} events.");
        
        // 打印所有录制的事件用于调试
        for (int i = 0; i < recordedEvents.Count; i++)
        {
            var evt = recordedEvents[i];
            Debug.Log($"Event {i}: {evt.ClickedObjectName} at time {evt.Time}, IsUIButton: {evt.IsUIButton}");
        }
    }

    private static void RecordButtonClick(Button button)
    {
        recordedEvents.Add(new RecordedEvent
        {
            Time = Time.realtimeSinceStartup - startTime,
            ClickedObjectName = button.gameObject.name,
            IsUIButton = true,
            MousePosition = Vector2.zero, // UI按钮不需要鼠标位置
            EventType = EventType.Used // 标记为UI事件
        });

        Debug.Log($"Recorded UGUI Button Click: {button.gameObject.name}");
    }

    private static int replayIndex;
    private static float replayStartTime;
    private static Action replayCompleteCallback;

    private static void ReplayEvents()
    {
        if (replayIndex >= recordedEvents.Count)
        {
            Debug.Log("Replay complete.");
            EditorApplication.update -= ReplayEvents;
            replayCompleteCallback?.Invoke();
            return;
        }

        float currentTime = Time.realtimeSinceStartup - replayStartTime;
        RecordedEvent currentEvent = recordedEvents[replayIndex];

        if (currentTime >= currentEvent.Time)
        {
            SimulateClick(currentEvent);
            replayIndex++;
        }
    }

    private static void SimulateClick(RecordedEvent recordedEvent)
    {
        if (recordedEvent.IsUIButton)
        {
            // 处理UGUI按钮点击
            GameObject clickedObject = GameObject.Find(recordedEvent.ClickedObjectName);
            if (clickedObject != null)
            {
                var button = clickedObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                    Debug.Log($"Replayed UGUI Button Click: {clickedObject.name}");
                }
                else
                {
                    Debug.LogWarning($"UGUI Button {recordedEvent.ClickedObjectName} not found during replay.");
                }
            }
        }
        else
        {
            // 处理普通场景对象点击
            Debug.Log($"Replaying Click: {recordedEvent.ClickedObjectName}");
            GameObject clickedObject = GameObject.Find(recordedEvent.ClickedObjectName);
            if (clickedObject != null)
            {
                Selection.activeGameObject = clickedObject;
            }
            else
            {
                Debug.LogWarning($"Object {recordedEvent.ClickedObjectName} not found during replay.");
            }
        }
    }

    private static void SaveRecordedEvents()
    {
        try
        {
            var data = new SerializableEventData
            {
                events = recordedEvents.Select(e => new SerializableEvent
                {
                    Time = e.Time,
                    ClickedObjectName = e.ClickedObjectName,
                    IsUIButton = e.IsUIButton,
                    MousePositionX = e.MousePosition.x,
                    MousePositionY = e.MousePosition.y,
                    EventTypeInt = (int)e.EventType
                }).ToArray()
            };

            string json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(DataPath, json);
            Debug.Log($"Saved {recordedEvents.Count} events to {DataPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save recorded events: {ex.Message}");
        }
    }

    private static void LoadRecordedEvents()
    {
        try
        {
            if (System.IO.File.Exists(DataPath))
            {
                string json = System.IO.File.ReadAllText(DataPath);
                var data = JsonUtility.FromJson<SerializableEventData>(json);
                
                recordedEvents.Clear();
                if (data?.events != null)
                {
                    foreach (var e in data.events)
                    {
                        recordedEvents.Add(new RecordedEvent
                        {
                            Time = e.Time,
                            ClickedObjectName = e.ClickedObjectName,
                            IsUIButton = e.IsUIButton,
                            MousePosition = new Vector2(e.MousePositionX, e.MousePositionY),
                            EventType = (EventType)e.EventTypeInt
                        });
                    }
                }
                
                Debug.Log($"Loaded {recordedEvents.Count} events from {DataPath}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load recorded events: {ex.Message}");
            recordedEvents.Clear();
        }
    }

    [System.Serializable]
    private class SerializableEventData
    {
        public SerializableEvent[] events;
    }

    [System.Serializable]
    private class SerializableEvent
    {
        public float Time;
        public string ClickedObjectName;
        public bool IsUIButton;
        public float MousePositionX;
        public float MousePositionY;
        public int EventTypeInt;
    }

    private class RecordedEvent
    {
        public float Time;
        public string ClickedObjectName;
        public bool IsUIButton; // 是否是UGUI按钮
        public Vector2 MousePosition; // 鼠标点击位置
        public EventType EventType; // 事件类型
    }
}
