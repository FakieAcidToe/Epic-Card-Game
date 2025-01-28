using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class DrawCard : MonoBehaviour
{
	XRSimpleInteractable deckInteractor;
	MeshRenderer meshRenderer;

	int playerNumber;

	[Tooltip("The card that will be spawned"), SerializeField]
	Card cardPrefab = null;
	[Tooltip("The layer mask of the spawned card"), SerializeField]
	InteractionLayerMask cardHandLayerMask;
	[Tooltip("The layer mask of the card when placed on the table"), SerializeField]
	InteractionLayerMask cardTableLayerMask;

	[Tooltip("The transform where the card is spanwed"), SerializeField]
	Transform spawnPosition = null;

	public bool canDraw = false;

	public List<CardsScriptableObj> cardsInDeck;

	public UnityEvent onDraw;

	void Awake()
	{
		deckInteractor = GetComponent<XRSimpleInteractable>();
		meshRenderer = GetComponent<MeshRenderer>();
		deckInteractor.selectEntered.AddListener(DrawACard);
		UpdateDeckHeight();
	}

	void OnDestroy()
	{
		deckInteractor.selectEntered.RemoveListener(DrawACard);
	}

	void DrawACard(SelectEnterEventArgs arg)
	{
		// ungrab the deck
		arg.manager.SelectCancel(arg.interactorObject, deckInteractor);

		Card newCard = InstantiateNewCard();

		// make interactor grab the card instead of the deck
		if (newCard != null)
			arg.manager.SelectEnter(arg.interactorObject, newCard.GetComponent<XRGrabInteractable>());
	}

	public void ForceDrawACard(bool overrideCanDraw = false)
	{
		Card newCard = InstantiateNewCard(overrideCanDraw);
		if (newCard != null)
			StartCoroutine(newCard.GoToLastSocketNextFrame());
	}

	Card InstantiateNewCard(bool overrideCanDraw = false)
	{
		if ((canDraw || overrideCanDraw) && cardsInDeck.Count > 0)
		{
			// create new card obj
			Card newCard = Instantiate(cardPrefab, spawnPosition.position, spawnPosition.rotation);

			newCard.SocketCard(CardGameManager.instance.GetPlayers()[playerNumber].duelDisk.AddSocket(), false); // tell the card to go to hand when dropped
			newCard.InitCardInfo(cardsInDeck[0]); // fill in card stats
			newCard.SetPlayerNum(playerNumber, true);
			newCard.SetLayerMasks(cardHandLayerMask, cardTableLayerMask);
			cardsInDeck.RemoveAt(0); // remove card from deck

			CardGameManager.instance.RegisterCard(newCard);
			UpdateDeckHeight(); // thin the deck obj

			canDraw = false;
			onDraw.Invoke();

			return newCard;
		}
		return null;
	}

	private void OnValidate()
	{
		if (!spawnPosition) spawnPosition = transform;
		//UpdateDeckHeight();
	}

	public void UpdateDeckHeight()
	{
		meshRenderer.enabled = cardsInDeck.Count > 0;
		if (meshRenderer.enabled) transform.parent.localScale = new Vector3(transform.parent.localScale.x,cardsInDeck.Count,transform.parent.localScale.z);
	}

	public void SetPlayerNumber(int _playerNumber)
	{
		playerNumber = _playerNumber;
	}

	public int GetPlayerNumber()
	{
		return playerNumber;
	}
}