using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Destroy All")]
public class DestroyAllEffect : CardEffect
{
	[SerializeField] bool destroySelf = false;
	[SerializeField] bool destroyHandCards = false;
	[SerializeField] bool destroyTableCards = true;
	[SerializeField] bool destroyOwnTeam = true;
	[SerializeField] bool destroyEnemyTeam = true;

	public override void ExecuteEffect(Card _thisCard)
	{
		int playerNum = _thisCard.GetPlayerNum();
		for (int i = CardGameManager.instance.allCardsInPlay.Count - 1; i >=0; --i)
		{
			Card card = CardGameManager.instance.allCardsInPlay[i];

			if (!destroySelf && card == _thisCard) continue;
			if (!destroyHandCards && card.GetSocket() is DuelDiskSocket) continue;
			if (!destroyTableCards && card.GetSocket() is DuelTableSocket) continue;
			if (!destroyOwnTeam && card.GetPlayerNum() == _thisCard.GetPlayerNum()) continue;
			if (!destroyEnemyTeam && card.GetPlayerNum() != _thisCard.GetPlayerNum()) continue;

			card.DestroyCard();
		}
	}
}