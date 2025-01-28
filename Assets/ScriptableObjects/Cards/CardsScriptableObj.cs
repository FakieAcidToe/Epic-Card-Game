using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "ScriptableObjects/Card", order = 1)]
public class CardsScriptableObj : ScriptableObject
{
	public Material colour;
	public string cardName;
	public Sprite artwork;
	public int cost;
	public int attack;
	public int health;
	[TextArea] public string effect;
	public List<CardAbility> cardAbilities;

	[System.Serializable]
	public struct CardAbility
	{
		public GameEvent activationTime;
		public List<CardEffect> costs;
		public List<CardEffect> effects;
	}
}