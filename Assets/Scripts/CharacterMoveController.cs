using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.VisualScripting;
using System;

public class CharacterMoveController : MonoBehaviour
{
    public EventTrigger eventTrigger;
    public delegate void PointerDownDelegate(BaseEventData data);
    public delegate void PointerUpDelegate(BaseEventData data);
    //public delegate void PointerClickDelegate(BaseEventData data);
    //public event PointerClickDelegate OnPointerClickEvent;
    public event PointerDownDelegate OnPointerDownEvent;
    public event PointerUpDelegate OnPointerUpEvent;
    public bool startReset = false;
    public float idleTimeReset = 10f;
    public float idleCount = 0f;

    void Start()
    {
        // Add a pointer click event
        //AddEventTrigger(eventTrigger, EventTriggerType.PointerClick, OnPointerClick);
        AddEventTrigger(eventTrigger, EventTriggerType.PointerDown, OnPointerDown);
        AddEventTrigger(eventTrigger, EventTriggerType.PointerUp, OnPointerUp);
    }

    private void Update()
    {
        if(this.startReset)
        {
            if(this.idleCount > 0f)
            {
                this.idleCount -= Time.deltaTime;
            }
            else
            {
                this.TriggerActive(true);
                this.resetIdle();
            }
        }
        else
        {
            this.resetIdle();
        }
    }

    // Function to add an event trigger
    void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    // Callback function for pointer click event
    /*void OnPointerClick(BaseEventData data)
    {
        //Debug.Log("Pointer Clicked!");
        // Invoke the event to call the function from Player class
        OnPointerClickEvent?.Invoke(data);
    }*/

    void OnPointerDown(BaseEventData data)
    {
        //Debug.Log("Pointer Down!");
        OnPointerDownEvent?.Invoke(data);
    }

    void OnPointerUp(BaseEventData data)
    {
       // Debug.Log("Pointer Up!");
        OnPointerUpEvent?.Invoke(data);
    }

    public void TriggerActive(bool active)
    {
        if(this.eventTrigger != null)
        {
            this.eventTrigger.GetComponent<Image>().DOColor(active? Color.white : Color.gray, 0f);
            this.eventTrigger.enabled = active;
            this.startReset = !active;
        }
    }

    public void PointerEffect(bool active)
    {
        if (this.eventTrigger != null)
        {
            this.eventTrigger.GetComponent<Image>().DOColor(active ? Color.gray : Color.white, 0f);
        }
    }


    void resetIdle()
    {
        this.idleCount = this.idleTimeReset;
    }
}
