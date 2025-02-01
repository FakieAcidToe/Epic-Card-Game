using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPlayerCPU : MonoBehaviour
{
	public IEnumerator PerformMainPhase(DuelField _player)
	{
		while (true) // play as many cards to front row as possible
		{
			yield return new WaitForSecondsRealtime(1f);
			Card card = GetPlayableCard(_player);
			DuelTableSocket freeSocket = GetFreeFrontRow(_player);
			if (card != null && freeSocket != null)
				card.PlayCard(freeSocket);
			else
				break;
		}
	}

	DuelTableSocket GetFreeFrontRow(DuelField _player)
	{
		foreach (DuelTableSocket socket in _player.frontTableSockets)
			if (socket.GetSocketedCard() == null)
				return socket;
		return null;
	}

	Card GetPlayableCard(DuelField _player)
	{
		List<Card> cardsInHand = _player.duelDisk.GetAllCardsInHand();
		return GetPlayableCard(cardsInHand);
	}

	Card GetPlayableCard(List<Card> _cardsInHand)
	{
		foreach (Card card in _cardsInHand)
			if (card.cardStats.cost <= 2)
				return card;
		return null;
	}
}