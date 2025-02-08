using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Power Up Others")]
public class PowerUpOthersEffect : CardEffect
{
	[SerializeField] int increaseAttackAmount;
	[SerializeField] bool powerSelf = false;
	[SerializeField] bool powerHandCards = false;
	[SerializeField] bool powerTableCards = true;
	[SerializeField] bool powerOwnTeam = true;
	[SerializeField] bool powerEnemyTeam = false;

	public override void ExecuteEffect(Card _thisCard)
	{
		int playerNum = _thisCard.GetPlayerNum();
		for (int i = CardGameManager.instance.allCardsInPlay.Count - 1; i >=0; --i)
		{
			Card card = CardGameManager.instance.allCardsInPlay[i];

			if (!powerSelf && card == _thisCard) continue;
			if (!powerHandCards && card.GetSocket() is DuelDiskSocket) continue;
			if (!powerTableCards && card.GetSocket() is DuelTableSocket) continue;
			if (!powerOwnTeam && card.GetPlayerNum() == _thisCard.GetPlayerNum()) continue;
			if (!powerEnemyTeam && card.GetPlayerNum() != _thisCard.GetPlayerNum()) continue;

			card.AttackStatChange(increaseAttackAmount);
		}
	}
}