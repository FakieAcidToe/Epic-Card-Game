using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DeckbuilderSpawner : MonoBehaviour
{
	[SerializeField] Card card;

	DeckBuilderManager manager;

	XRGrabInteractable cardInteractable;

	void Awake()
	{
		cardInteractable = card.GetComponent<XRGrabInteractable>();
		cardInteractable.selectEntered.AddListener(DrawACard);
	}

	void OnDestroy()
	{
		cardInteractable.selectEntered.RemoveListener(DrawACard);
	}

	public void InitSpawner(DeckBuilderManager _manager, InteractionLayerMask _cardLayerMask, CardsScriptableObj _cardStats)
	{
		manager = _manager;
		card.InitCardInfo(_cardStats);
		card.SetLayerMasks(_cardLayerMask, _cardLayerMask);
	}

	void DrawACard(SelectEnterEventArgs arg)
	{
		// ungrab this card
		arg.manager.SelectCancel(arg.interactorObject, cardInteractable);

		Card newCard = manager.InstantiateNewCard(card.cardStats, card.transform, false);

		// make interactor grab the new card
		if (newCard != null)
			arg.manager.SelectEnter(arg.interactorObject, newCard.GetComponent<XRGrabInteractable>());
	}
}
