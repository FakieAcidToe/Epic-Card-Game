using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Power Up")]
public class PowerUpEffect : CardEffect
{
	[SerializeField] int increaseAttackAmount;
	[SerializeField] bool fieldOnly;

	public override void ExecuteEffect(Card _thisCard)
	{
		if (fieldOnly && _thisCard.GetSocket() is not DuelTableSocket) return;

		_thisCard.AttackStatChange(increaseAttackAmount);
	}
}