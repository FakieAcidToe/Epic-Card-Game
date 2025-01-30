using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class Card : MonoBehaviour
{
	public enum CardState
	{
		hand,
		table
	}

	XRGrabInteractable interactable;
	Animator animator;
	DuelSocket lastSocket;
	public CardsScriptableObj cardStats;

	CardState cardState = CardState.hand;
	InteractionLayerMask handCardMask;
	InteractionLayerMask tableCardMask;
	int playerNumber;
	int origPlayerNumber;

	int attack;
	int health;

	IEnumerator socketCoroutine; // for the 2 sec throwing

	public bool canBeMoved = false;

	[Header("Card Part References")]
	[SerializeField] MeshRenderer cardFront;
	[SerializeField] TextMeshPro cardName;
	[SerializeField] SpriteRenderer cardArt;
	[SerializeField] SpriteRenderer cardHologramArt;
	[SerializeField] TextMeshPro cardCost;
	[SerializeField] TextMeshPro cardAttack;
	[SerializeField] TextMeshPro cardHealth;
	[SerializeField] TextMeshPro cardEffect;
	List<CardsScriptableObj.CardAbility> cardAbilities;

	[Header("Card Events")]
	[SerializeField] GameEvent selfSummonEvent;
	[SerializeField] GameEvent anySummonEvent;
	[SerializeField] GameEvent selfAttackingEvent;
	[SerializeField] GameEvent anyAttackingEvent;
	[SerializeField] GameEvent endPhaseEvent;

	void Awake()
	{
		interactable = GetComponent<XRGrabInteractable>();
		animator = GetComponent<Animator>();
		interactable.selectEntered.AddListener(RememberSocket);
		interactable.lastSelectExited.AddListener(DoGoToLastSocket);

		InitCardInfo();
		ChangeState(CardState.hand);
	}

	void OnDestroy()
	{
		interactable.selectEntered.RemoveListener(RememberSocket);
		interactable.lastSelectExited.RemoveListener(DoGoToLastSocket);
	}

	void ExecuteEffects(List<CardEffect> _effects)
	{
		foreach (CardEffect effect in _effects)
			effect.ExecuteEffect(this);
	}

	void SummonActions()
	{
		ChangeState(CardState.table);

		//selfSummonEvent.Raise();
		anySummonEvent.Raise();

		foreach (CardsScriptableObj.CardAbility ability in cardAbilities)
		{
			if (ability.activationTime == selfSummonEvent)
				ExecuteEffects(ability.effects);
		}
	}

	public void EndPhase()
	{
		foreach (CardsScriptableObj.CardAbility ability in cardAbilities)
		{
			if (ability.activationTime == endPhaseEvent)
				ExecuteEffects(ability.effects);
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
				Card potentialExistingCard = newSocket.GetSocketedCard();

				if (canBeMoved && potentialExistingCard == null) // empty case: check if new socket currently has no card
				{
					// handle destroying/unlinking old sockets
					if (lastSocket != null)
					{
						if (lastSocket is DuelDiskSocket) // if summoned from hand
							SummonActions();

						// unlink old duel socket
						lastSocket.UnsocketCard();
					}

					// remember this new socket for later
					SocketCard(newSocket, false);
				}
				else if (canBeMoved && potentialExistingCard != null && lastSocket != null && lastSocket is DuelTableSocket) // swap case: if new socket has card, and this grabbed card has a table socket
				{
					DuelSocket oldSocket = lastSocket;

					// unlink old duel socket
					lastSocket.UnsocketCard();
					newSocket.UnsocketCard();

					// remember new socket for later
					SocketCard(newSocket, false);
					potentialExistingCard.SocketCard(oldSocket, false);
				}
				else // failed case: if new socket currently has a card
				{
					// tell card to go back to old socket
					socketInteractor.interactionManager.SelectCancel((IXRSelectInteractor)socketInteractor, interactable);
					GoToLastSocket();
				}
			}
		}
	}

	public void UpdateHologramStatus()
	{
		animator.SetBool("showHologram", lastSocket == null ? false : lastSocket.shouldShowHologram);
	}

	public void Attack()
	{
		animator.SetTrigger("attackTrigger");

		//selfAttackingEvent.Raise();
		anyAttackingEvent.Raise();

		foreach (CardsScriptableObj.CardAbility ability in cardAbilities)
		{
			if (ability.activationTime == selfAttackingEvent)
				ExecuteEffects(ability.effects);
		}

		CardGameManager manager = CardGameManager.instance;
		manager.DamagePlayer(attack);
	}

	void DoGoToLastSocket(SelectExitEventArgs arg)
	{
		if (socketCoroutine != null)
			StopCoroutine(socketCoroutine);
		socketCoroutine = GoToLastSocket2Sec();

		if (gameObject.activeInHierarchy) StartCoroutine(socketCoroutine);
	}

	public IEnumerator GoToLastSocketNextFrame()
	{
		yield return new WaitForNextFrameUnit();
		GoToLastSocket();
	}

	public IEnumerator GoToLastSocket2Sec()
	{
		yield return new WaitForSecondsRealtime(2);
		GoToLastSocket();
	}

	public void GoToLastSocket()
	{
		if (interactable != null && !interactable.isSelected && lastSocket != null)
			lastSocket.SocketCard(this, true);
	}

	public void InitCardInfo(CardsScriptableObj _cardStats)
	{
		cardStats = _cardStats;
		InitCardInfo();
	}

	public void InitCardInfo()
	{
		if (cardStats == null) return;
		cardFront.material = cardStats.colour;
		cardName.text = cardStats.name;
		cardCost.text = cardStats.cost.ToString();
		cardArt.sprite = cardStats.artwork;
		cardHologramArt.sprite = cardStats.artwork;
		cardAttack.text = cardStats.attack.ToString();
		cardHealth.text = cardStats.health.ToString();
		cardEffect.text = cardStats.effect.ToString();
		cardAbilities = cardStats.cardAbilities;

		attack = cardStats.attack;
		health = cardStats.health;
	}

	public void UpdateCardInteractionLayerMask()
	{
		switch (cardState)
		{
			case CardState.hand:
				interactable.interactionLayers = handCardMask;
				break;
			default:
			case CardState.table:
				interactable.interactionLayers = tableCardMask;
				break;
		}
	}

	public void ChangeState(CardState _newState)
	{
		cardState = _newState;
		UpdateCardInteractionLayerMask();
	}

	public void SetLayerMasks(InteractionLayerMask handMask, InteractionLayerMask tableMask)
	{
		handCardMask = handMask;
		tableCardMask = tableMask;
		UpdateCardInteractionLayerMask();
	}

	public void SocketCard(DuelSocket _socket, bool _alsoForceSelectEnter = false)
	{
		lastSocket = _socket;
		lastSocket.SocketCard(this, _alsoForceSelectEnter);
		UpdateHologramStatus();
	}

	public void DestroyCard()
	{
		CardGameManager.instance.UnregisterCard(this);
		lastSocket.UnsocketCard();
		Destroy(gameObject);
	}

	public void ReturnToHand()
	{
		if (lastSocket is not DuelDiskSocket)
		{
			lastSocket.UnsocketCard();
			SocketCard(CardGameManager.instance.GetPlayers()[playerNumber].duelDisk.AddSocket(), true); // tell the card to go to hand when dropped
			ChangeState(CardState.hand);
		}
	}

	public IEnumerator ReturnToHandNextFrame()
	{
		yield return new WaitForNextFrameUnit();
		ReturnToHand();
	}

	public DuelSocket GetSocket()
	{
		return lastSocket;
	}

	public void SetPlayerNum(int _playerNumber, bool _alsoSetOrigPlayer = false)
	{
		playerNumber = _playerNumber;
		if (_alsoSetOrigPlayer) origPlayerNumber = _playerNumber;
	}

	public int GetPlayerNum()
	{
		return playerNumber;
	}

	public int GetOrigPlayerNum()
	{
		return origPlayerNumber;
	}
}