using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageDisplayer : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public float messageDisplayTime = 1f;
    public float fadeTime = 0.5f;

    private Queue<Message> m_MessageQueue = new Queue<Message>();
    private bool m_IsDisplayingMessages;

    public struct Message
    {
        public string message;
        public Color color;

        public Message(string message)
        {
            this.message = message;
            this.color = Color.white;
        }

        public Message(string message, Color color)
        {
            this.message = message;
            this.color = color;
        }
    }

    private IEnumerator DisplayQueuedMessagesRoutine()
    {
        messageText.gameObject.SetActive(true);

        while (m_MessageQueue.Count > 0)
        {
            var currentMessage = m_MessageQueue.Dequeue();

            yield return StartCoroutine(DisplayMessageRoutine(currentMessage));
        }

        messageText.gameObject.SetActive(false);
        m_IsDisplayingMessages = false;
    }

    private IEnumerator DisplayMessageRoutine(Message currentMessage)
    {
        messageText.text = currentMessage.message;
        Color messageColor = currentMessage.color;
        messageText.color = messageColor;

        float delta = 0;
        float t;
        while (true)
        {
            t = delta / fadeTime;
            messageColor.a = Mathf.Lerp(0, 1, t);
            messageText.color = messageColor;

            delta += Time.deltaTime;
            
            if (t >= 1f)
            {
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(messageDisplayTime);

        delta = fadeTime;
        while(true)
        {
            t = delta / fadeTime;
            messageColor.a = Mathf.Lerp(0, 1, t);
            messageText.color = messageColor;

            delta -= Time.deltaTime;

            if (t <= 0f)
            {
                break;
            }
            yield return null;
        }
    }

    public void RegisterMessage(string message)
    {
        RegisterMessage(new Message(message));
    }

    public void RegisterMessage(string message, Color messageColor)
    {
        RegisterMessage(new Message(message, messageColor));
    }

    public void RegisterMessage(Message message)
    {
        m_MessageQueue.Enqueue(message);

        if (!m_IsDisplayingMessages)
        {
            m_IsDisplayingMessages = true;
            StartCoroutine(DisplayQueuedMessagesRoutine());
        }
    }
}
