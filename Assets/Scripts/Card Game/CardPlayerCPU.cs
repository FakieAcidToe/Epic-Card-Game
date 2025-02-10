using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPlayerCPU : MonoBehaviour
{
	public IEnumerator PerformMainPhase(DuelField _player)
	{
		DuelField opponentField = CardGameManager.instance.GetPlayers()[CardGameManager.instance.GetNextTurnPlayerNum()];
		List<Card> allMyCards = GetPlayableCards(_player);
		int[] cardPos = new int[allMyCards.Count];
		uint availableMana = _player.GetMana();

		// calculate threat levels
		int[] threat = { 0, 0, 0 };
		for (int i = 0; i < 3; ++i)
		{
			DuelTableSocket socket = opponentField.frontTableSockets[2 - i];
			Card card = socket.GetSocketedCard() as Card;
			if (card != null)
				threat[i] = card.GetAttack();
		}

		// find order to deal with threat
		int[] order = { 0, 1, 2 }; // order is reversed btw
		Array.Sort(threat, order);

		// choose cards to deal with highest attack first
		for (int i = order.Length - 1; i >= 0; --i)
		{
			Card opponentCard = opponentField.frontTableSockets[2 - i].GetSocketedCard() as Card;

			int cardValue = 0;
			int cardPosIndex = -1;
			uint cardCost = 0;
			for (int j = 0; j < allMyCards.Count; ++j)
			{
				int thisCardValue = ((allMyCards[j].GetAttack() >= (opponentCard == null ? 1 : opponentCard.GetHealth()))?20:0) + allMyCards[j].GetHealth();
				if (cardPos[j] == 0 && thisCardValue > cardValue)
				{
					if (allMyCards[j].GetSocket() is DuelTableSocket)
					{
						cardValue = thisCardValue;
						cardPosIndex = j;
						cardCost = 0;
					}
					else if (allMyCards[j].GetSocket() is DuelDiskSocket && allMyCards[j].cardStats.cost <= availableMana)
					{
						cardValue = thisCardValue;
						cardPosIndex = j;
						cardCost = (uint)allMyCards[cardPosIndex].cardStats.cost;
					}
				}
			}
			if (cardPosIndex > -1)
			{
				cardPos[cardPosIndex] = 1 + i;
				availableMana -= cardCost;
			}
		}

		// actually play the cards
		for (int i = 0; i < allMyCards.Count; ++i)
		{
			if (cardPos[i] > 0)
			{
				Card card = allMyCards[i];
				DuelTableSocket freeSocket = null;

				if (cardPos[i] <= 3)
					freeSocket = _player.frontTableSockets[cardPos[i] - 1];
				else if (cardPos[i] <= 6)
					freeSocket = _player.backTableSockets[cardPos[i] - 4];

				if (freeSocket != null)
				{
					Card existingCard = freeSocket.GetSocketedCard() as Card;
					if (card.GetSocket() is DuelDiskSocket && existingCard != null)
					{
						yield return new WaitForSecondsRealtime(1f);
						DuelTableSocket freeBackrow = GetFreeBackRow(_player);
						if (freeBackrow != null)
							existingCard.PlayCard(freeBackrow);
						else
							existingCard.PlayCard(_player.discardSocket);
					}

					if (card != null)
					{
						yield return new WaitForSecondsRealtime(1f);
						card.PlayCard(freeSocket);
					}
				}
			}
		}
		yield return new WaitForSecondsRealtime(1f);
	}

	DuelTableSocket GetFreeBackRow(DuelField _player)
	{
		foreach (DuelTableSocket socket in _player.backTableSockets)
			if (socket.GetSocketedCard() == null)
				return socket;
		return null;
	}

	List<Card> GetPlayableCards(DuelField _player)
	{
		List<Card> allCards = new List<Card>();

		foreach (DuelTableSocket socket in _player.frontTableSockets)
		{
			Card card = socket.GetSocketedCard() as Card;
			if (card != null)
				allCards.Add(card);
		}

		foreach (DuelTableSocket socket in _player.backTableSockets)
		{
			Card card = socket.GetSocketedCard() as Card;
			if (card != null)
				allCards.Add(card);
		}

		allCards.AddRange(_player.duelDisk.GetAllCardsInHand());

		return allCards;
	}
}