using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
	public GameEvent Event;
	public UnityEvent Response;

	void OnEnable()
	{
		Event.RegisterListener(this);
	}

	void OnDisable()
	{
		Event.UnRegisterListener(this);
	}

	public void OnEventRaised()
	{
		Response?.Invoke();
	}
}