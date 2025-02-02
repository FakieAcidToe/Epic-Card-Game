using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Burn Damage")]
public class BurnEffect : CardEffect
{
	[SerializeField] int burnAmount;

	public override void ExecuteEffect(Card _thisCard)
	{
		Debug.Log("Dealing " + burnAmount + " damage.");
	}
}