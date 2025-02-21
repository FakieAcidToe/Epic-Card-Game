using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CardGameManager : MonoBehaviour
{
	public static CardGameManager instance { get; private set; }

	[SerializeField] List<DuelField> players = new List<DuelField>();
	[SerializeField] TextMeshProUGUI phaseText;

	[SerializeField] uint numStartingCards = 3;
	[SerializeField] uint startingLifePoints = 20;
	[SerializeField] uint startingMana = 4;
	[SerializeField] int manaGainPerTurn = 2;

	[SerializeField] int turnPlayer = 0;
	uint turnCount = 0;

	TurnPhase phase = TurnPhase.Start;

	public List<Card> allCardsInPlay { get; private set; }
	CardPlayerCPU cpu;

	[SerializeField] GameEvent endPhaseEvent;
	[SerializeField] UnityEvent onVictoryAnnounce;
	[SerializeField] UnityEvent onDefeatAnnounce;
	[SerializeField] UnityEvent onGameEnd;

	public enum TurnPhase
	{
		Start, // draw 5(?) cards to start the game
		DrawPhase, // draw 1 card
		MainPhase, // do stuff
		AttackPhase, // attack stuff
		EndPhase, // pass to next player in draw phase
		Finish // game ended
	}

	void Awake()
	{
		if (instance != null && instance != this) Destroy(this);
		else instance = this;
	}

	void Start()
	{
		allCardsInPlay = new List<Card>();
		cpu = GetComponent<CardPlayerCPU>();

		for (int i = 0; i < players.Count; ++i)
		{
			players[i].deck.SetDeck(players[i].isPlayer ? SaveFileSystem.Instance.deck : SaveFileSystem.Instance.GetNewStarterDeck());
			players[i].SetPlayerNumber(i);
			players[i].InitLifeMana(startingLifePoints, startingMana);
			players[i].HintArrowActive(false);
		}

		SetPhase(TurnPhase.Start);
	}

	void OnValidate()
	{
		// update life points text
		for (int i = 0; i < players.Count; ++i)
			if (players[i] != null)
				players[i].InitLifeMana(startingLifePoints, startingMana);
	}

	void NextTurnPlayer()
	{
		turnPlayer = GetNextTurnPlayerNum();
		++turnCount;
	}
	public int GetNextTurnPlayerNum()
	{
		return (turnPlayer + 1) % players.Count;
	}

	public List<DuelField> GetPlayers()
	{
		return players;
	}

	public void SetPhase(TurnPhase _phase)
	{
		phase = _phase;
		UpdateCardLock();
		switch (phase)
		{
			case TurnPhase.Start:
				StartCoroutine(StartPhaseCoroutine());
				break;
			case TurnPhase.DrawPhase:
				StartCoroutine(DrawPhaseCoroutine());
				break;
			case TurnPhase.MainPhase:
				StartCoroutine(MainPhaseCoroutine());
				break;
			case TurnPhase.AttackPhase:
				StartCoroutine(AttackPhaseCoroutine());
				break;
			case TurnPhase.EndPhase:
				StartCoroutine(EndPhaseCoroutine());
				break;
			case TurnPhase.Finish:
				StartCoroutine(FinishCoroutine());
				break;
		}
	}

	void UpdateCardLock()
	{
		switch (phase)
		{
			default:
				// lock all Cards
				foreach (Card card in allCardsInPlay)
					card.canBeMoved = false;
				break;
			case TurnPhase.MainPhase:
				// lock opponent Cards only
				foreach (Card card in allCardsInPlay)
					card.canBeMoved = turnPlayer == card.GetPlayerNum();
				break;
		}
	}

	public void DamagePlayer(int _damage)
	{
		DamagePlayer(_damage, GetNextTurnPlayerNum());
	}

	public void DamagePlayer(int _damage, int _playerNum)
	{
		players[_playerNum].DamagePlayer(_damage);
	}

	public bool ConsumeMana(int _amount)
	{
		return ConsumeMana(_amount, turnPlayer);
	}

	public bool ConsumeMana(int _amount, int _playerNum)
	{
		return players[_playerNum].ConsumeMana(_amount);
	}

	public void UpdateAllLifePointText()
	{
		for (int i = 0; i < players.Count; ++i)
			players[i].UpdateLifePointText();
	}

	public void ControllerButtonPressed(int _playerNumber)
	{
		if (_playerNumber == turnPlayer)
			switch (phase)
			{
				case TurnPhase.DrawPhase:
					players[turnPlayer].deck.ForceDrawACard(false);
					break;
				case TurnPhase.MainPhase:
				case TurnPhase.AttackPhase:
					players[turnPlayer].button.ForcePressButton();
					break;
			}
	}

	IEnumerator WaitForAllDiscs()
	{
		// wait for all duel discs to load
		while (true)
		{
			bool allLoaded = true;
			foreach (DuelField player in players)
				if (player.duelDisk == null || !player.duelDisk.isActiveAndEnabled)
				{
					allLoaded = false;
					break;
				}
			if (allLoaded)
				break;
			yield return new WaitForNextFrameUnit();
		}
	}

	IEnumerator WaitForAllCardsToGoSocket()
	{
		// wait for all cards to be on their lastSocket
		while (true)
		{
			bool allOnSocket = true;
			foreach (Card card in allCardsInPlay)
				if (!card.IsOnSocket())
				{
					allOnSocket = false;
					break;
				}
			if (allOnSocket)
				break;
			yield return new WaitForNextFrameUnit();
		}
	}

	IEnumerator StartPhaseCoroutine()
	{
		phaseText.text = "Start!";

		yield return StartCoroutine(WaitForAllDiscs());
		yield return new WaitForSeconds(3f);

		// draw 5 (numStartingCards) cards each
		for (uint i = 0; i < numStartingCards; ++i)
		{
			foreach (DuelField player in players) player.deck.ForceDrawACard(true);
			yield return new WaitForSeconds(0.2f);
		}

		// next phase
		SetPhase(TurnPhase.DrawPhase);
	}

	IEnumerator DrawPhaseCoroutine()
	{
		phaseText.text = "Draw Phase";

		// gain mana
		if (turnCount > 0) // dont gain mana on turn 0
			ConsumeMana(-manaGainPerTurn);

		if (turnCount == 0 || players[turnPlayer].deck.cardsInDeck.Count <= 0) // dont draw on turn 0
		{
			yield return new WaitForSeconds(1);
		}
		else
		{
			if (players[turnPlayer].isPlayer)
			{
				players[turnPlayer].HintArrowActive(true);
				players[turnPlayer].deck.canDraw = true;

				// wait for player to draw a card
				while (players[turnPlayer].deck.canDraw)
					yield return new WaitForNextFrameUnit();

				players[turnPlayer].HintArrowActive(false);
			}
			else
			{
				yield return new WaitForSeconds(1);
				ForceDrawACard(turnPlayer);
			}
		}

		yield return StartCoroutine(WaitForAllCardsToGoSocket());

		// next phase
		SetPhase(TurnPhase.MainPhase);
	}

	IEnumerator WaitUntilEvent(UnityEvent unityEvent)
	{
		bool trigger = false;
		Action action = () => trigger = true;
		unityEvent.AddListener(action.Invoke);
		yield return new WaitUntil(() => trigger);
		unityEvent.RemoveListener(action.Invoke);
	}

	public void UpdateTurnPlayerCardMaterial()
	{
		// set all material of cards in hand based on mana
		foreach (Card card in allCardsInPlay)
			if (turnPlayer == card.GetPlayerNum() && card.GetSocket() is DuelDiskSocket)
				card.CardMaterialEnable(card.cardStats.cost <= players[turnPlayer].GetMana());
	}

	IEnumerator MainPhaseCoroutine()
	{
		phaseText.text = "Main Phase";
		UpdateTurnPlayerCardMaterial();

		if (players[turnPlayer].isPlayer)
		{
			// wait for player to press the button
			yield return StartCoroutine(WaitUntilEvent(players[turnPlayer].button.onPress));
		}
		else
		{
			yield return StartCoroutine(cpu.PerformMainPhase(players[turnPlayer]));
			players[turnPlayer].button.ForcePressButton();
		}

		// next phase
		if (turnCount == 0) // dont attack on turn 0
			SetPhase(TurnPhase.EndPhase);
		else
			SetPhase(TurnPhase.AttackPhase);
	}

	IEnumerator AttackPhaseCoroutine()
	{
		phaseText.text = "Attack Phase";

		yield return new WaitForSeconds(0.5f);

		List<DuelTableSocket> sockets = players[turnPlayer].frontTableSockets;
		int targetPlayerNum = GetNextTurnPlayerNum();

		for (int i = 0; i < sockets.Count; ++i)
		{
			Card attackingCard = sockets[i].GetSocketedCard() as Card;
			if (attackingCard != null)
				yield return StartCoroutine(attackingCard.Attack(targetPlayerNum, sockets.Count - 1 - i));
			yield return new WaitForSeconds(0.3f);
		}

		yield return new WaitForSeconds(1);

		// next phase
		SetPhase(TurnPhase.EndPhase);
	}

	IEnumerator EndPhaseCoroutine()
	{
		phaseText.text = "End Phase";

		yield return new WaitForSeconds(0.5f);

		endPhaseEvent.Raise();

		yield return new WaitForSeconds(0.5f);

		if (players[GetNextTurnPlayerNum()].GetLife() == 0)
		{
			// opponent died
			SetPhase(TurnPhase.Finish);
		}
		else
		{
			// next phase
			NextTurnPlayer();
			SetPhase(TurnPhase.DrawPhase);
		}
	}

	IEnumerator FinishCoroutine()
	{
		if (players[0].GetLife() == 0)
		{
			phaseText.text = "Defeat...";
			onDefeatAnnounce.Invoke();
		}
		else
		{
			phaseText.text = "Victory!";
			onVictoryAnnounce.Invoke();
		}

		yield return new WaitForSeconds(5f);
		onGameEnd.Invoke();
	}

	public void RegisterCard(Card _card)
	{
		allCardsInPlay.Add(_card);
	}

	public void UnregisterCard(Card _card)
	{
		allCardsInPlay.Remove(_card);
	}

	public void ForceDrawACard(int _playerNumber)
	{
		if (_playerNumber >= 0 && _playerNumber < players.Count)
			players[_playerNumber].deck.ForceDrawACard(true);
		UpdateCardLock();
	}
}