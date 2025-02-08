using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DeckBuilderManager : MonoBehaviour
{
	[SerializeField] List<DuelTableSocket> sockets;
	[SerializeField] Card cardPrefab;
	[SerializeField] InteractionLayerMask cardLayerMask;
	[SerializeField] Vector3 cardSpawnPosOffset;
	[SerializeField] Vector3 cardSpawnRotOffset;

	List<Card> allCardsInPlay;

	void Start()
	{
		allCardsInPlay = new List<Card>();
		InstantiateCurrentDeckCards();
	}

	public void InstantiateCurrentDeckCards()
	{
		// remove all existing cards
		for (int i = allCardsInPlay.Count-1; i >= 0; --i)
		{
			allCardsInPlay[i].GetSocket().UnsocketCard();
			Destroy(allCardsInPlay[i].gameObject);
			allCardsInPlay.Remove(allCardsInPlay[i]);
		}

		for (int i = 0; i < sockets.Count; ++i)
		{
			Transform socketTransform = sockets[i].transform;
			CardsScriptableObj cardStats = SaveFileSystem.Instance.deck[i];

			// create new card obj
			Card newCard = Instantiate(cardPrefab, socketTransform.position + cardSpawnPosOffset, Quaternion.Euler(socketTransform.rotation.eulerAngles + cardSpawnRotOffset));

			newCard.canBeMoved = true;
			newCard.SetLayerMasks(cardLayerMask, cardLayerMask);
			newCard.SocketCard(sockets[i], true);
			newCard.InitCardInfo(cardStats); // fill in card stats

			allCardsInPlay.Add(newCard);
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
}