using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/Return to Hand")]
public class ReturnToHandEffect : CardEffect
{
	public override void ExecuteEffect(Card _thisCard)
	{
		_thisCard.StartCoroutine(_thisCard.ReturnToHandNextFrame());
	}
}