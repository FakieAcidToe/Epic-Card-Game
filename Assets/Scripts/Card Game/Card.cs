using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class Card : CardBase
{
	public enum CardState
	{
		hand,
		table
	}

	Animator animator;
	[Header("Cardgame Card variables")]
	public CardsScriptableObj cardStats;

	CardState cardState = CardState.hand;
	InteractionLayerMask handCardMask;
	InteractionLayerMask tableCardMask;
	int playerNumber;
	int origPlayerNumber;

	int attack;
	int health;

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

	[Header("Card SFX")]
	[SerializeField] AudioClip cardSacrificeSound;

	[Header("Prefab References")]
	[SerializeField] DamageNumbers damageNumberPrefab;
	[SerializeField] Transform damageNumberSpawnPosition;

	void Awake()
	{
		interactable = GetComponent<XRGrabInteractable>();
		animator = GetComponent<Animator>();
		audioQuickSound = GetComponent<PlayQuickSound>();
		interactable.selectEntered.AddListener(OnSelectEnter);
		interactable.lastSelectExited.AddListener(OnSelectExit);

		InitCardInfo();
		ChangeState(CardState.hand);
	}

	void OnDestroy()
	{
		interactable.selectEntered.RemoveListener(OnSelectEnter);
		interactable.lastSelectExited.RemoveListener(OnSelectExit);
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

	void OnSelectEnter(SelectEnterEventArgs arg)
	{
		if (arg.interactorObject is XRSocketInteractor)
		{
			RememberSocket(arg);
		}
	}

	public void SacrificeCard(bool forceSacrifice = false)
	{
		if (canBeMoved || forceSacrifice)
		{
			AudioSource.PlayClipAtPoint(cardSacrificeSound, transform.position, audioQuickSound.volume);
			CardGameManager.instance.ConsumeMana(-1);
			DestroyCard(false);
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
				Card potentialExistingCard = newSocket.GetSocketedCard() as Card;

				if (canBeMoved && newSocket is DuelDiscardSocket)
				{
					SacrificeCard();
				}
				else if (canBeMoved && potentialExistingCard == null && lastSocket != null && lastSocket is DuelDiskSocket) // summon from hand case
				{
					if (CardGameManager.instance.ConsumeMana(cardStats.cost)) // mana cost
					{
						SummonActions();
						DuelTableSocket tableSocket = newSocket as DuelTableSocket;
						if (tableSocket != null)
						{
							if (cardStats.shouldSpawnParticles) tableSocket.ParticleBurst(cardStats.spawnParticleColour);
							if (cardStats.spawnParticleSprite != null) tableSocket.PlaySpriteParticle(cardStats.spawnParticleSprite);
						}
						PlayAudio(cardPlaySound);

						// unlink old duel socket
						lastSocket.UnsocketCard();
		
						// remember this new socket for later
						SocketCard(newSocket, false);
					}
					else // insuffucient mana
					{
						// tell card to go back to old socket
						socketInteractor.interactionManager.SelectCancel((IXRSelectInteractor)socketInteractor, interactable);
						GoToLastSocket();
					}
				}
				else if (canBeMoved && potentialExistingCard != null && lastSocket != null && lastSocket is DuelTableSocket) // swap case: if new socket has card, and this grabbed card has a table socket
				{
					PlayAudio(cardPlaySound);

					DuelSocket oldSocket = lastSocket;
		
					// unlink old duel socket
					lastSocket.UnsocketCard();
					newSocket.UnsocketCard();
		
					// remember new socket for later
					SocketCard(newSocket, false);
					potentialExistingCard.SocketCard(oldSocket, false);
				}
				else if (canBeMoved && potentialExistingCard == null) // success case: empty socket
				{
					PlayAudio(cardPlaySound);

					if (lastSocket != null)
						lastSocket.UnsocketCard(); // unlink old socket
		
					// remember this new socket for later
					SocketCard(newSocket, false);
				}
				else  // failed case: if new socket currently has a card that's unswappable, or, !canBeMoved
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

	public IEnumerator Attack(int _playerNum, int _slotNum)
	{
		animator.SetTrigger("attackTrigger");
		PlayAudio(cardStats.attackWhooshSound);

		//selfAttackingEvent.Raise();
		anyAttackingEvent.Raise();

		foreach (CardsScriptableObj.CardAbility ability in cardAbilities)
		{
			if (ability.activationTime == selfAttackingEvent)
				ExecuteEffects(ability.effects);
		}

		// wait for animation to hit
		yield return new WaitForSeconds(0.15f);

		// deal damage
		CardGameManager manager = CardGameManager.instance;
		DuelField damagedPlayer = manager.GetPlayers()[_playerNum];
		DuelTableSocket damagedSocket = damagedPlayer.frontTableSockets[_slotNum];
		Card damagedCard = damagedSocket.GetSocketedCard() as Card;
		int damageAmount = attack;

		if (damageAmount > 0)
		{
			damagedSocket.PlaySpriteParticle(cardStats.attackParticleSprite);
			PlayAudio(cardStats.attackHitSound);
		}

		if (damagedCard != null) // damage defending card
		{
			damageAmount = damagedCard.TakeDamage(damageAmount);

			if (damageAmount > 0) // bleedthrough excess damage
			{
				damagedSocket = damagedPlayer.backTableSockets[_slotNum];
				damagedCard = damagedSocket.GetSocketedCard() as Card;

				damagedSocket.PlaySpriteParticle(cardStats.attackParticleSprite);
				if (damagedCard != null) // damage 2nd defending card
					damagedCard.TakeDamage(damageAmount);
				else // no 2nd defending card
					manager.DamagePlayer(attack, _playerNum);
			}
		}
		else // no defending card
			manager.DamagePlayer(attack, _playerNum);
	}

	public int TakeDamage(int _damage)
	{
		int excessDamage = _damage - health;
		if (excessDamage < 0) excessDamage = 0;

		health -= _damage;
		if (health < 0) health = 0;
		cardHealth.text = health.ToString(); // update text


		if (_damage > 0) InstantiateDamageNumber(_damage);

		if (health == 0) DestroyCard();
		else if (_damage > 0) animator.SetTrigger("hurtTrigger");

		return excessDamage; // return extra damage for bleedthrough
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

	public new void DestroyCard(bool _playDestroySfx = true)
	{
		CardGameManager.instance.UnregisterCard(this);
		lastSocket.UnsocketCard();

		if (_playDestroySfx) AudioSource.PlayClipAtPoint(cardDestroySound, transform.position, audioQuickSound.volume);
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

	void InstantiateDamageNumber(int _damage)
	{
		DamageNumbers numbers = Instantiate(damageNumberPrefab, damageNumberSpawnPosition.position, damageNumberSpawnPosition.rotation);
		numbers.SetDamageNumber(_damage);
	}
}