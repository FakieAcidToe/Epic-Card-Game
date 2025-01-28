using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class DuelSocket : MonoBehaviour
{
	public bool shouldShowHologram = false;
	Card currentSocketedCard;

	XRSocketInteractor interactor;
	DuelDisk ownerDisk;

	void Awake()
	{
		interactor = GetComponent<XRSocketInteractor>();
	}

	public void SetInteractionLayerMask(InteractionLayerMask _layers)
	{
		interactor.interactionLayers = _layers;
	}

	public void SocketCard(Card _card, bool _alsoForceSelectEnter = true)
	{
		if (currentSocketedCard == null || currentSocketedCard == _card)
		{
			if (_alsoForceSelectEnter) interactor.interactionManager.SelectEnter((IXRSelectInteractor)interactor, _card.GetComponent<XRGrabInteractable>());
			currentSocketedCard = _card;
		}
		else
			Debug.LogError("DuelSocket attempted to socket a card while already having a card.");
	}

	public void UnsocketCard()
	{
		if (currentSocketedCard != null)
		{
			XRGrabInteractable cardInteractable = currentSocketedCard.GetComponent<XRGrabInteractable>();
			if (cardInteractable.IsSelected(interactor))
				interactor.interactionManager.SelectCancel((IXRSelectInteractor)interactor, cardInteractable);
			currentSocketedCard = null;
		}
		else
			Debug.LogError("DuelSocket attempted to unsocket a card while not having a card.");
	}

	public XRSocketInteractor GetInteractor()
	{
		return interactor;
	}

	public Card GetSocketedCard()
	{
		return currentSocketedCard;
	}

	public void SetDuelDisk(DuelDisk _ownerDisk)
	{
		ownerDisk = _ownerDisk;
	}

	public DuelDisk GetDuelDisk()
	{
		return ownerDisk;
	}
}