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

	[SerializeField] List<Player> players = new List<Player>();
	[SerializeField] TextMeshProUGUI phaseText;

	[SerializeField] uint numStartingCards = 5;
	[SerializeField] uint startingLifePoints = 20;
	[SerializeField] string lifePointsPrependText = "Life: ";

	[SerializeField] int turnPlayer = 0;
	uint turnCount = 0;

	uint[] lifePoints;

	TurnPhase phase = TurnPhase.Start;

	public List<Card> allCardsInPlay { get; private set; }

	[SerializeField] GameEvent endPhaseEvent;

	public enum TurnPhase
	{
		Start, // draw 5(?) cards to start the game
		DrawPhase, // draw 1 card
		MainPhase, // do stuff
		AttackPhase, // attack stuff
		EndPhase // pass to next player in draw phase
	}

	[System.Serializable]
	public struct Player
	{
		public bool isPlayer;
		public DuelDisk duelDisk;
		public DrawCard deck;
		public ButtonFollowVisual button;
		public TextMeshProUGUI lifeText;
	}

	void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(this);
		}
		else
		{
			instance = this;

			allCardsInPlay = new List<Card>();

			lifePoints = new uint[players.Count];

			for (int i = 0; i < players.Count; ++i)
			{
				players[i].duelDisk.SetPlayerNumber(i);
				players[i].deck.SetPlayerNumber(i);

				lifePoints[i] = startingLifePoints;
			}

			UpdateLifePointText();
		}
	}

	void Start()
	{
		SetPhase(TurnPhase.Start);
	}

	void OnValidate()
	{
		// update life points text
		for (int i = 0; i < players.Count; ++i)
			if (players[i].lifeText != null)
				players[i].lifeText.text = lifePointsPrependText + startingLifePoints.ToString();
	}

	void NextTurnPlayer()
	{
		++turnPlayer;
		turnPlayer %= players.Count;

		++turnCount;
	}

	public List<Player> GetPlayers()
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

	public void UpdateLifePointText()
	{
		for (int i = 0; i < players.Count; ++i)
			players[i].lifeText.text = lifePointsPrependText + lifePoints[i].ToString();
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
			foreach (Player player in players)
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

	IEnumerator StartPhaseCoroutine()
	{
		phaseText.text = "Start!";

		yield return StartCoroutine(WaitForAllDiscs());

		// draw 5 (numStartingCards) cards each
		for (uint i = 0; i < numStartingCards; ++i)
		{
			foreach (Player player in players) player.deck.ForceDrawACard(true);
			yield return new WaitForSecondsRealtime(0.2f);
		}

		// next phase
		SetPhase(TurnPhase.DrawPhase);
	}

	IEnumerator DrawPhaseCoroutine()
	{
		phaseText.text = "Draw Phase";

		if (turnCount == 0 || players[turnPlayer].deck.cardsInDeck.Count <= 0) // dont draw on turn 0
		{
			yield return new WaitForSecondsRealtime(1);
		}
		else
		{
			if (players[turnPlayer].isPlayer)
			{
				players[turnPlayer].deck.canDraw = true;

				// wait for player to draw a card
				while (players[turnPlayer].deck.canDraw)
					yield return new WaitForNextFrameUnit();
			}
			else
			{
				yield return new WaitForSecondsRealtime(1);
				ForceDrawACard(turnPlayer);
			}
		}

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

	IEnumerator MainPhaseCoroutine()
	{
		phaseText.text = "Main Phase";

		if (players[turnPlayer].isPlayer)
		{
			// wait for player to press the button
			yield return StartCoroutine(WaitUntilEvent(players[turnPlayer].button.onPress));
		}
		else
		{
			yield return new WaitForSecondsRealtime(1);
			players[turnPlayer].button.ForcePressButton();
		}

		// next phase
		SetPhase(TurnPhase.AttackPhase);
	}

	IEnumerator AttackPhaseCoroutine()
	{
		phaseText.text = "Attack Phase";

		yield return new WaitForSecondsRealtime(0.5f);

		for (int i = 0; i < allCardsInPlay.Count; ++i)
		{
			Card card = allCardsInPlay[i];
			DuelTableSocket socket = card.GetSocket() as DuelTableSocket;
			if (socket != null && socket.GetIsFrontRow() && turnPlayer == card.GetPlayerNum())
			{
				card.Attack();
				yield return new WaitForSecondsRealtime(0.1f);
			}
		}
		yield return new WaitForSecondsRealtime(1);

		// next phase
		SetPhase(TurnPhase.EndPhase);
	}

	IEnumerator EndPhaseCoroutine()
	{
		phaseText.text = "End Phase";

		yield return new WaitForSecondsRealtime(0.5f);

		endPhaseEvent.Raise();

		yield return new WaitForSecondsRealtime(0.5f);

		// next phase
		NextTurnPlayer();
		SetPhase(TurnPhase.DrawPhase);
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