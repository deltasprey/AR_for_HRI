using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

//[RequireComponent(typeof(TMP_Text))]
public class DebugLogPanel : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField]
    private bool filterLogLevel = true;

    [Tooltip("Granularity. Sometimes you may not want to see everything being sent to the console.")]
    [SerializeField]
    LogType LogLevel;

    [Tooltip("Maximum number of messages before deleting the older messages.")]
    [SerializeField]
    private int maxNumberOfMessages=15;

    [Tooltip("Check this if you want the stack trace printed after the message.")]
    [SerializeField]
    private bool includeStackTrace=false;

    [Header("Auditory Feedback")]
    [Tooltip("Play a sound when the message panel is updated.")]
    [SerializeField]
    private bool playSoundOnMessage;

    private bool newMessageArrived = false;

    private string stringColor;

    private TMP_Text debugText;

    // The queue with the messages:
    private Queue<string> messageQueue;

    // The message sound, should you use one
    private AudioSource messageSound;

    void OnEnable()
    {
        messageQueue = new Queue<string>();       
        debugText = GetComponent<TMP_Text>();
        Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
        messageSound = this.GetComponent<AudioSource>();
    }
   

    private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
    {        
        if (!filterLogLevel || (filterLogLevel && type == LogLevel))
        {

            if (messageSound!=null && playSoundOnMessage)
            {
                messageSound.Play();
            }

            newMessageArrived = true;

            switch (type) {
                case LogType.Error:
                    stringColor = "red";
                    break;
                case LogType.Warning:
                    stringColor = "yellow";
                    break;
                case LogType.Exception:
                    stringColor = "orange";
                    break;
                case LogType.Assert:
                    stringColor = "olive";
                    break;
                default:
                    stringColor = "white";
                    break;
            }

            StringBuilder stringBuilder = new();

            stringBuilder.Append("\n");
            stringBuilder.Append($"<color={stringColor}>{type}: ");
            stringBuilder.Append(condition);
            stringBuilder.Append("</color>");

            if (includeStackTrace)
            {
                stringBuilder.Append("\nStackTrace: ");
                stringBuilder.Append(stackTrace);
            }

            condition = stringBuilder.ToString();
            messageQueue.Enqueue(condition);
        
            if (maxNumberOfMessages > 0 && messageQueue.Count > maxNumberOfMessages)
            {
                messageQueue.Dequeue();
            }
        }
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= Application_logMessageReceivedThreaded;
    }

    /// <summary>
    /// Print the queue to the text mesh.
    /// </summary>

    void PrintQueue()
    {
        StringBuilder stringBuilder = new();
        string[] messageList = messageQueue.ToArray();

        for (int i = 0; i < messageList.Length; i++) {
            stringBuilder.Append(messageList[i]);
            stringBuilder.Append("\n");
        }        

        string message = stringBuilder.ToString();
        debugText.text = message;

        Invoke(nameof(UpdateViewport), 0.5f);
    }

    void UpdateViewport() {
        if (debugText.isTextOverflowing) {
            debugText.rectTransform.sizeDelta = new Vector2(debugText.rectTransform.sizeDelta.x, debugText.rectTransform.sizeDelta.y + 14f);
            debugText.rectTransform.localPosition = new Vector3(0, debugText.rectTransform.sizeDelta.y - 80, 0);
        }
    }

    /// <summary>
    /// This Update method checks if a new message has arrived. The check is placed here to ensure
    /// that only the main thread will try to access the Text Mesh.
    /// </summary>

    void Update()
    {
        if (newMessageArrived)
        {
            PrintQueue();
            newMessageArrived = false;
        }
    }
}
