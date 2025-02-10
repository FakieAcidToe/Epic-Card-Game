using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class CardBase : MonoBehaviour
{
	protected XRGrabInteractable interactable;
	protected PlayQuickSound audioQuickSound;
	protected DuelSocket lastSocket;

	protected IEnumerator socketCoroutine; // for the 2 sec throwing

	[Header("Base Card variables")]
	public bool canBeMoved = false;

	[Header("Base Card SFX")]
	[SerializeField] protected AudioClip cardPlaySound;
	[SerializeField] protected AudioClip cardDestroySound;

	void Awake()
	{
		interactable = GetComponent<XRGrabInteractable>();
		audioQuickSound = GetComponent<PlayQuickSound>();
		interactable.selectEntered.AddListener(OnSelectEnter);
		interactable.lastSelectExited.AddListener(OnSelectExit);
	}

	void OnDestroy()
	{
		interactable.selectEntered.RemoveListener(OnSelectEnter);
		interactable.lastSelectExited.RemoveListener(OnSelectExit);
	}

	void OnSelectEnter(SelectEnterEventArgs arg)
	{
		if (arg.interactorObject is XRSocketInteractor)
		{
			RememberSocket(arg);
		}
	}

	void RememberSocket(SelectEnterEventArgs arg)
	{
		if (socketCoroutine != null)
			StopCoroutine(socketCoroutine);
		
		XRSocketInteractor socketInteractor = arg.interactorObject as XRSocketInteractor;
		if (socketInteractor != null) // if interactor is a socket (not hand controller)
		{
			DuelSocket newSocket = socketInteractor.GetComponent<DuelSocket>();

			if (lastSocket != newSocket) // if old and new sockets are different
			{
				CardBase potentialExistingCard = newSocket.GetSocketedCard();

				if (canBeMoved && potentialExistingCard == null) // success case: empty socket
				{
					PlayAudio(cardPlaySound);

					if (lastSocket != null)
						lastSocket.UnsocketCard(); // unlink old socket
		
					// remember this new socket for later
					SocketCard(newSocket, false);
				}
				else  // failed case
				{
					// tell card to go back to old socket
					socketInteractor.interactionManager.SelectCancel((IXRSelectInteractor)socketInteractor, interactable);
					GoToLastSocket();
				}
			}
		}
	}

	void OnSelectExit(SelectExitEventArgs arg)
	{
		if (!gameObject.activeSelf) return;

		if (arg.interactorObject is XRSocketInteractor)
		{
			StartCoroutine(GoToLastSocketNextFrame());
		}
		else if (arg.interactorObject is NearFarInteractor)
		{
			if (socketCoroutine != null)
				StopCoroutine(socketCoroutine);
			socketCoroutine = GoToLastSocket2Sec();

			if (gameObject.activeInHierarchy) StartCoroutine(socketCoroutine);
		}
	}

	public IEnumerator GoToLastSocketNextFrame()
	{
		yield return new WaitForNextFrameUnit();
		GoToLastSocket();
	}

	public IEnumerator GoToLastSocket2Sec()
	{
		yield return new WaitForSeconds(2);
		GoToLastSocket();
	}

	public void GoToLastSocket()
	{
		if (interactable != null && !interactable.isSelected && lastSocket != null)
			lastSocket.SocketCard(this, true);
	}

	public void SocketCard(DuelSocket _socket, bool _alsoForceSelectEnter = false)
	{
		lastSocket = _socket;
		lastSocket?.SocketCard(this, _alsoForceSelectEnter);
	}

	public void PlayCard(DuelSocket _socket)
	{
		CardBase existingCard = _socket.GetSocketedCard();
		if (existingCard == null)
			_socket.SocketCard(this, true);
		else if (_socket != lastSocket)
		{
			lastSocket.UnsocketCard();
			_socket.UnsocketCard();

			lastSocket.SocketCard(existingCard, true);
			_socket.SocketCard(this, true);
		}
	}

	public void DestroyCard(bool _playDestroySfx = true)
	{
		lastSocket.UnsocketCard();

		if (_playDestroySfx) AudioSource.PlayClipAtPoint(cardDestroySound, transform.position, audioQuickSound.volume);
		Destroy(gameObject);
	}

	public DuelSocket GetSocket()
	{
		return lastSocket;
	}

	public void PlayAudio(AudioClip _audioClip)
	{
		audioQuickSound.sound = _audioClip;
		audioQuickSound.Play();
	}
}