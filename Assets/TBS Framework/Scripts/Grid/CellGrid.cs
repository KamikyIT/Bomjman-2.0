using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using TbsFramework.Cells;
using TbsFramework.Grid.GridStates;
using TbsFramework.Grid.UnitGenerators;
using TbsFramework.Players;
using TbsFramework.Units;

namespace TbsFramework.Grid
{
    /// <summary>
    /// CellGrid class keeps track of the game, stores cells, units and players objects. It starts the game and makes turn transitions. 
    /// It reacts to user interacting with units or cells, and raises events related to game progress. 
    /// </summary>
    public class CellGrid : MonoBehaviour
    {
        /// <summary>
        /// LevelLoading event is invoked before Initialize method is run.
        /// </summary>
        public event Action<CellGrid> LevelLoading;
        /// <summary>
        /// LevelLoadingDone event is invoked after Initialize method has finished running.
        /// </summary>
        public event Action<CellGrid> LevelLoadingDone;
        /// <summary>
        /// GameStarted event is invoked at the beggining of StartGame method.
        /// </summary>
        public event Action<CellGrid> GameStarted;
        /// <summary>
        /// GameEnded event is invoked when there is a single player left in the game.
        /// </summary>
        public event Action<CellGrid> GameEnded;
        /// <summary>
        /// Turn ended event is invoked at the end of each turn.
        /// </summary>
        public event Action<CellGrid> TurnEnded;

        /// <summary>
        /// UnitAdded event is invoked each time AddUnit method is called.
        /// </summary>
        public event EventHandler<UnitCreatedEventArgs> UnitAdded;

        CellGridState _cellGridState; //The grid delegates some of its behaviours to cellGridState object.
        public CellGridState CellGridState
        {
            get
            {
                return _cellGridState;
            }
            set
            {
                if (_cellGridState != null)
                    _cellGridState.OnStateExit();
                _cellGridState = value;
                _cellGridState.OnStateEnter();
            }
        }

        int NumberOfPlayers { get; set; }

        public Player CurrentPlayer
        {
            get { return Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)); }
        }
        public int CurrentPlayerNumber { get; private set; }


        Transform _playersParent;
        /// <summary>
        /// GameObject that holds player objects.
        /// </summary>
        Transform PlayersParent {
            get {
                if (_playersParent == null)
                {
                    var playersParentGo = GameObject.FindGameObjectWithTag(Helpers.TagsManager.PlayersParent);
                    if (playersParentGo == null)
                        Debug.LogError("_playersParentGo == null. Mark PlayersParent GameObject with 'PlayersParent' Tag.");

                    _playersParent = playersParentGo.transform;
                }
                return _playersParent;
            }
        }

        List<Player> Players { get; set; }
        public List<Cell> Cells { get; private set; }
        public List<Unit> Units { get; private set; }

        private void Start()
        {
            LevelLoading?.Invoke(this);

            Initialize();

            LevelLoadingDone?.Invoke(this);

            StartGame();
        }

        void Initialize()
        {
            Players = new List<Player>();
            for (int i = 0; i < PlayersParent.childCount; i++)
            {
                var player = PlayersParent.GetChild(i).GetComponent<Player>();
                if (player != null)
                {
                    if (Players.Any(x => x.PlayerNumber == player.PlayerNumber))
                        Debug.LogError($"Same PlayerNumber '{player.PlayerNumber}' in Players.");
                    Players.Add(player);
                }
                else
                    Debug.LogError("Invalid object in Players Parent game object");
            }
            NumberOfPlayers = Players.Count;
            CurrentPlayerNumber = Players.Min(p => p.PlayerNumber);

            Cells = new List<Cell>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var cell = transform.GetChild(i).gameObject.GetComponent<Cell>();
                if (cell != null)
                    Cells.Add(cell);
                else
                    Debug.LogError("Invalid object in cells parent game object");
            }

            foreach (var cell in Cells)
            {
                cell.CellClicked += OnCellClicked;
                cell.CellHighlighted += OnCellHighlighted;
                cell.CellDehighlighted += OnCellDehighlighted;
                cell.GetNeighbours(Cells);
            }

            Units = new List<Unit>();
            var unitGenerator = GetComponent<IUnitGenerator>();
            if (unitGenerator != null)
            {
                var units = unitGenerator.SpawnUnits(Cells);
                foreach (var unit in units)
                {
                    Units.Add(unit);
                    unit.UnitClicked += OnUnitClicked;
                    unit.UnitDestroyed += OnUnitDestroyed;

                    UnitAdded?.Invoke(this, new UnitCreatedEventArgs(unit.transform));
                }
            }
            else
                Debug.LogError("No IUnitGenerator script attached to cell grid");
        }

        private void OnCellDehighlighted(Cell cell)
        {
            CellGridState.OnCellDeselected(cell);
        }
        private void OnCellHighlighted(Cell cell)
        {
            CellGridState.OnCellSelected(cell);
        }
        private void OnCellClicked(Cell cell)
        {
            CellGridState.OnCellClicked(cell);
        }

        private void OnUnitClicked(object sender, EventArgs e)
        {
            CellGridState.OnUnitClicked(sender as Unit);
        }
        private void OnUnitDestroyed(object sender, AttackEventArgs e)
        {
            Units.Remove(sender as Unit);
            var totalPlayersAlive = Units.Select(u => u.PlayerNumber).Distinct().Count(); //Checking if the game is over
            if (totalPlayersAlive == 1)
                GameEnded?.Invoke(this);
        }

        /// <summary>
        /// Method is called once, at the beggining of the game.
        /// </summary>
        public void StartGame()
        {
            GameStarted?.Invoke(this);

            Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => u.OnTurnStart());
            Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)).Play(this);
            Debug.Log("Game started");
        }

        /// <summary>
        /// Method makes turn transitions. It is called by player at the end of his turn.
        /// </summary>
        public void EndTurn()
        {
            if (Units.Select(u => u.PlayerNumber).Distinct().Count() == 1)
            {
                return;
            }
            CellGridState = new CellGridStateTurnChanging(this);

            Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnEnd(); });

            CurrentPlayerNumber = (CurrentPlayerNumber + 1) % NumberOfPlayers;
            while (Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).Count == 0)
            {
                CurrentPlayerNumber = (CurrentPlayerNumber + 1) % NumberOfPlayers;
            }//Skipping players that are defeated.

            TurnEnded?.Invoke(this);

            Debug.Log(string.Format("Player {0} turn", CurrentPlayerNumber));
            Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnStart(); });
            Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)).Play(this);
        }
    }
}

