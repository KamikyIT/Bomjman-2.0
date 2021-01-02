using System;
using TbsFramework.Cells;
using TbsFramework.Grid;
using TbsFramework.Players;
using TbsFramework.Units;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TbsFramework.Example1
{
    public class GuiController : MonoBehaviour
    {
        public CellGrid CellGrid;
        public Button NextTurnButton;

        public Image UnitImage;
        public Text InfoText;
        public Text StatsText;

        void Awake()
        {
            UnitImage.color = Color.gray;

            CellGrid.GameStarted += OnGameStarted;
            CellGrid.TurnEnded += OnTurnEnded;
            CellGrid.GameEnded += OnGameEnded;
            CellGrid.UnitAdded += OnUnitAdded;
        }

        private void OnGameStarted(CellGrid cellGrid)
        {
            foreach (Transform cell in CellGrid.transform)
            {
                cell.GetComponent<Cell>().CellHighlighted += OnCellHighlighted;
                cell.GetComponent<Cell>().CellDehighlighted += OnCellDehighlighted;
            }

            OnTurnEnded(cellGrid);
        }

        private void OnGameEnded(CellGrid cellGrid)
        {
            InfoText.text = "Player " + (cellGrid.CurrentPlayerNumber + 1) + " wins!";
        }
        private void OnTurnEnded(CellGrid cellGrid)
        {
            NextTurnButton.interactable = cellGrid.CurrentPlayer is HumanPlayer;

            InfoText.text = "Player " + cellGrid.CurrentPlayerNumber + 1;
        }
        private void OnCellDehighlighted(Cell cell)
        {
            UnitImage.color = Color.gray;
            StatsText.text = "";
        }
        private void OnCellHighlighted(Cell cell)
        {
            UnitImage.color = Color.gray;
            StatsText.text = "Movement Cost: " + cell.MovementCost;
        }
        private void OnUnitAttacked(object sender, AttackEventArgs e)
        {
            if (!(CellGrid.CurrentPlayer is HumanPlayer)) return;
            OnUnitDehighlighted(sender, new EventArgs());

            if ((sender as Unit).HitPoints <= 0) return;

            OnUnitHighlighted(sender, e);
        }
        private void OnUnitDehighlighted(object sender, EventArgs e)
        {
            StatsText.text = "";
            UnitImage.color = Color.gray;
        }
        private void OnUnitHighlighted(object sender, EventArgs e)
        {
            var unit = sender as MyUnit;
            StatsText.text = unit.UnitName + "\nHit Points: " + unit.HitPoints + "/" + unit.TotalHitPoints + "\nAttack: " + unit.AttackFactor + "\nDefence: " + unit.DefenceFactor + "\nRange: " + unit.AttackRange;
            UnitImage.color = unit.PlayerColor;

        }
        private void OnUnitAdded(object sender, UnitCreatedEventArgs e)
        {
            RegisterUnit(e.unit);
        }

        private void RegisterUnit(Transform unit)
        {
            unit.GetComponent<Unit>().UnitHighlighted += OnUnitHighlighted;
            unit.GetComponent<Unit>().UnitDehighlighted += OnUnitDehighlighted;
            unit.GetComponent<Unit>().UnitAttacked += OnUnitAttacked;
        }
        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
