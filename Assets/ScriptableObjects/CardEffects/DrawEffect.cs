using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Draw Cards")]
public class DrawEffect : CardEffect
{
	public int drawAmount;

	public override void ExecuteEffect(Card _thisCard)
	{
		int playerNum = _thisCard.GetPlayerNum();
		for (int i = 0; i < drawAmount; ++i)
		{
			CardGameManager.instance.ForceDrawACard(playerNum);
		}
	}
}