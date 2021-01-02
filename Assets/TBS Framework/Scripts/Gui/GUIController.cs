using System;
using TbsFramework.Grid;
using TbsFramework.Grid.GridStates;
using UnityEngine;

namespace TbsFramework.Gui
{
    public class GUIController : MonoBehaviour
    {
        public CellGrid CellGrid;

        void Awake()
        {
            CellGrid.LevelLoading += OnLevelLoading;
            CellGrid.LevelLoadingDone += OnLevelLoadingDone;
        }

        void OnLevelLoading(CellGrid cellGrid)
        {
            Debug.Log("Level is loading");
        }

        void OnLevelLoadingDone(CellGrid cellGrid)
        {
            Debug.Log("Level loading done");
            Debug.Log("Press 'n' to end turn");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.N) && !(CellGrid.CellGridState is CellGridStateAiTurn))
            {
                CellGrid.EndTurn();//User ends his turn by pressing "n" on keyboard.
            }
        }
    }
}