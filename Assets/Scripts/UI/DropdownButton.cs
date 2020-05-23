using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DropdownButton : MonoBehaviour
{
    public GameObject backgroundPanel;
    public Button[] buttons;
    public float waitDelay = 0.5f;
    public bool IsMenuOpen
    {
        get => m_Animator.GetBool("slide");
    }

    private Animator m_Animator;
    private float m_Timer;
    private bool m_IsBusy = false;
    private Coroutine m_CurrentRoutine;

    void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (m_IsBusy)
        {
            m_Timer += Time.deltaTime;

            if (m_Timer >= waitDelay)
            {
                m_IsBusy = false;
                m_Timer = 0f;
            }
        }
    }

    private IEnumerator SetButtonsInteractable()
    {
        yield return new WaitForSeconds(2f);

        foreach (var button in buttons)
        {
            button.interactable = true;
        }
    }

    public void TriggerAnimation()
    {
        if (m_IsBusy) return;

        m_IsBusy = true;

        bool isPanelOpen = m_Animator.GetBool("slide");
        if(m_CurrentRoutine!= null)
        {
            StopCoroutine(m_CurrentRoutine);
        }

        if(isPanelOpen)
        {
            m_Animator.SetBool("slide", false);
            backgroundPanel.SetActive(false);

            foreach (var button in buttons)
            {
                button.interactable = false;
            }
        }
        else
        {
            m_Animator.SetBool("slide", true);
            backgroundPanel.SetActive(true);
            m_CurrentRoutine = StartCoroutine(SetButtonsInteractable());
        }
    }

    public void RegisterEventToButton(int buttonIndex, UnityAction registeredEvent)
    {
#if UNITY_EDITOR
        if (buttonIndex > buttons.Length - 1)
        {
            Debug.LogError($"[{gameObject.name}] Trying to register event. But provided index is {buttonIndex}. But dropdown have {buttons.Length} button component.");
            return;
        }
#endif

        buttons[buttonIndex].onClick.AddListener(registeredEvent);
    }
}
