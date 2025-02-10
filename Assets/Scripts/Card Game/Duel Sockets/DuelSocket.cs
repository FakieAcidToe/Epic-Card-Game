using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class DuelSocket : MonoBehaviour
{
	public bool shouldShowHologram = false;
	public UnityEvent onCardSocket;

	CardBase currentSocketedCard;

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

	public void SocketCard(CardBase _card, bool _alsoForceSelectEnter = true)
	{
		if (currentSocketedCard == null || currentSocketedCard == _card)
		{
			if (_card == null) Debug.LogError("DuelSocket attempted to socket a null card.");
			else
			{
				if (_alsoForceSelectEnter) interactor.interactionManager.SelectEnter((IXRSelectInteractor)interactor, _card.GetComponent<XRGrabInteractable>());
				currentSocketedCard = _card;
				onCardSocket.Invoke();
			}
		}
		else Debug.LogError("DuelSocket attempted to socket a card while already having a card.");
	}

	public void UnsocketCard()
	{
		if (currentSocketedCard != null)
		{
			XRGrabInteractable cardInteractable = currentSocketedCard.GetComponent<XRGrabInteractable>();
			if (cardInteractable.IsSelected(interactor))
				interactor.interactionManager.SelectCancel((IXRSelectInteractor)interactor, cardInteractable);
			currentSocketedCard = null;

			// if is duel disk: destroy old duel disk socket
			DuelDiskSocket duelSocket = this as DuelDiskSocket;
			if (duelSocket != null)
			{
				DuelDisk duelDisk = duelSocket.GetDuelDisk();
				duelDisk.sockets.Remove(duelSocket);
				duelDisk.UpdateDuelDisk();
				Destroy(duelSocket.gameObject);
			}
		}
		else
			Debug.LogError("DuelSocket attempted to unsocket a card while not having a card.");
	}

	public XRSocketInteractor GetInteractor()
	{
		return interactor;
	}

	public CardBase GetSocketedCard()
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