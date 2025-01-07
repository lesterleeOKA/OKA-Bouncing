using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CharacterMoveController : MonoBehaviour
{
    public EventTrigger eventTrigger;
    public delegate void PointerClickDelegate(BaseEventData data);
    public event PointerClickDelegate OnPointerClickEvent;

    void Start()
    {
        // Add a pointer click event
        AddEventTrigger(eventTrigger, EventTriggerType.PointerClick, OnPointerClick);
    }

    // Function to add an event trigger
    void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    // Callback function for pointer click event
    void OnPointerClick(BaseEventData data)
    {
        //Debug.Log("Pointer Clicked!");
        // Invoke the event to call the function from Player class
        OnPointerClickEvent?.Invoke(data);
    }

    public void TriggerActive(bool active)
    {
        if(this.eventTrigger != null)
        {
            this.eventTrigger.GetComponent<Image>().DOFade(active? 1f : 0f, 0f);
            this.eventTrigger.enabled = active;
        }
    }
}
