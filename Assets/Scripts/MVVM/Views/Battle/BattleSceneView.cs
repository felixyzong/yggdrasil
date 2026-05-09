using MVVM.ViewModels.Battle;
using UnityEngine;

namespace MVVM.Views.Battle
{
    public class BattleSceneView : MonoBehaviour
    {
        [SerializeField] private BattleGridView gridView;
        [SerializeField] private BattleCommandBarView commandBarView;
        [SerializeField] private TurnPreviewView turnPreviewView;
        [SerializeField] private bool startSampleBattleOnStart = true;

        public BattleViewModel ViewModel { get; private set; }

        private void Awake()
        {
            ViewModel = new BattleViewModel();
            BindChildren();
        }

        private void Start()
        {
            if (startSampleBattleOnStart)
            {
                ViewModel.StartSampleBattle();
                BindChildren();
            }
        }

        private void BindChildren()
        {
            if (gridView != null)
            {
                gridView.Bind(ViewModel);
            }

            if (commandBarView != null)
            {
                commandBarView.Bind(ViewModel);
            }

            if (turnPreviewView != null)
            {
                turnPreviewView.Bind(ViewModel.TurnPreview);
            }
        }
    }
}
