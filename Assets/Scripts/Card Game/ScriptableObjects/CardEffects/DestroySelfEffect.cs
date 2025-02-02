using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Destroy Self")]
public class DestroySelfEffect : CardEffect
{
	public override void ExecuteEffect(Card _thisCard)
	{
		_thisCard.DestroyCard();
	}
}