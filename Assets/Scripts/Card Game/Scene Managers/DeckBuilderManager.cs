using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DeckBuilderManager : MonoBehaviour
{
	[Header("Table Sockets")]
	[SerializeField] List<DuelTableSocket> sockets;

	[Header("Card Spawners")]
	[SerializeField] DeckbuilderSpawner spawnerPrefab;
	[SerializeField] Transform spawnerOrigin; // spawners will be placed about this point
	[SerializeField] float spawnerRadius; // spawners will be placed this distance away from spawnerOrigin

	[Header("Table Cards")]
	[SerializeField] Card cardPrefab;
	[SerializeField] InteractionLayerMask cardLayerMask;
	[SerializeField] Vector3 cardSpawnPosOffset;
	[SerializeField] Vector3 cardSpawnRotOffset;

	List<Card> allCardsInPlay;

	void Start()
	{
		allCardsInPlay = new List<Card>();
		InstantiateCurrentDeckCards();
		InstantiateSpawners();
	}

	void InstantiateSpawners()
	{
		List<CardsScriptableObj> cardDex = SaveFileSystem.Instance.GetCardDex();
		for (int i = 0; i < cardDex.Count; ++i)
		{
			Quaternion rot = Quaternion.AngleAxis(i * 360f / cardDex.Count, spawnerOrigin.up);

			DeckbuilderSpawner spawner = Instantiate(spawnerPrefab, spawnerOrigin.position + rot * spawnerOrigin.forward * spawnerRadius, rot);
			spawner.InitSpawner(this, cardLayerMask, cardDex[i]);
		}
	}

	public void InstantiateCurrentDeckCards()
	{
		// remove all existing cards
		for (int i = allCardsInPlay.Count-1; i >= 0; --i)
		{
			allCardsInPlay[i].GetSocket()?.UnsocketCard();
			Destroy(allCardsInPlay[i].gameObject);
			allCardsInPlay.Remove(allCardsInPlay[i]);
		}

		for (int i = 0; i < sockets.Count; ++i)
		{
			CardsScriptableObj cardStats = SaveFileSystem.Instance.deck[i];

			Card newCard = InstantiateNewCard(cardStats, sockets[i].transform);
			newCard.SocketCard(sockets[i], true);
		}
	}

	public void SaveCards()
	{
		List<CardsScriptableObj> cards = new List<CardsScriptableObj>();
		foreach (DuelTableSocket socket in sockets)
		{
			cards.Add((socket.GetSocketedCard() as Card).cardStats);
		}
		SaveFileSystem.Instance.deck = cards;
		SaveFileSystem.Instance.SaveFile();
	}

	public Card InstantiateNewCard(CardsScriptableObj _cardStats, Transform spawnPosition, bool useCardSpawnOffset = true)
	{
		// create new card obj
		Card newCard;
		if (useCardSpawnOffset)
			newCard = Instantiate(cardPrefab, spawnPosition.position + cardSpawnPosOffset, Quaternion.Euler(spawnPosition.rotation.eulerAngles + cardSpawnRotOffset));
		else
			newCard = Instantiate(cardPrefab, spawnPosition.position, spawnPosition.rotation);

		newCard.canBeMoved = true;
		newCard.SetLayerMasks(cardLayerMask, cardLayerMask);
		newCard.InitCardInfo(_cardStats); // fill in card stats
		allCardsInPlay.Add(newCard);

		return newCard;
	}
}