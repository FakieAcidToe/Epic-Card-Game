using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
	public abstract void ExecuteEffect(Card _thisCard);
}