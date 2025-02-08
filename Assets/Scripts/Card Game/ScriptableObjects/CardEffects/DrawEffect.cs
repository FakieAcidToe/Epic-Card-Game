using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Draw Cards")]
public class DrawEffect : CardEffect
{
	[SerializeField] int drawAmount;
	[SerializeField] bool fieldOnly;

	public override void ExecuteEffect(Card _thisCard)
	{
		if (fieldOnly && _thisCard.GetSocket() is not DuelTableSocket) return;

		int playerNum = _thisCard.GetPlayerNum();
		for (int i = 0; i < drawAmount; ++i)
		{
			CardGameManager.instance.ForceDrawACard(playerNum);
		}
	}
}