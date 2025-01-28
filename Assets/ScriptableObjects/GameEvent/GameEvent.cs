using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "ScriptableObjects/Game Event", order = 2)]
public class GameEvent : ScriptableObject
{
	List<GameEventListener> gameEventListeners = new List<GameEventListener>();

	public void RegisterListener(GameEventListener _listener)
	{
		gameEventListeners.Add(_listener);
	}

	public void UnRegisterListener(GameEventListener _listener)
	{
		gameEventListeners.Remove(_listener);
	}

	public void Raise()
	{
		for (int i = gameEventListeners.Count - 1; i >= 0; --i)
		{
			gameEventListeners[i]?.OnEventRaised();
		}
	}
}