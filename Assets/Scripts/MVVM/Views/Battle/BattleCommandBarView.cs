using System.ComponentModel;
using MVVM.ViewModels.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.Views.Battle
{
    public class BattleCommandBarView : MonoBehaviour
    {
        [SerializeField] private Button moveButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Text moveButtonText;
        [SerializeField] private Text attackButtonText;
        [SerializeField] private Text endTurnButtonText;
        [SerializeField] private Text modeText;
        [SerializeField] private Text errorText;

        private BattleViewModel battleViewModel;
        private BattleCommandBarViewModel commandBarViewModel;

        public void Bind(BattleViewModel viewModel)
        {
            if (battleViewModel != null)
            {
                battleViewModel.PropertyChanged -= OnBattleViewModelChanged;
            }

            if (commandBarViewModel != null)
            {
                commandBarViewModel.PropertyChanged -= OnCommandBarChanged;
            }

            battleViewModel = viewModel;
            commandBarViewModel = viewModel?.CommandBar;

            if (battleViewModel != null)
            {
                battleViewModel.PropertyChanged += OnBattleViewModelChanged;
            }

            if (commandBarViewModel != null)
            {
                commandBarViewModel.PropertyChanged += OnCommandBarChanged;
            }

            WireButtons();
            Refresh();
        }

        private void OnDestroy()
        {
            if (battleViewModel != null)
            {
                battleViewModel.PropertyChanged -= OnBattleViewModelChanged;
            }

            if (commandBarViewModel != null)
            {
                commandBarViewModel.PropertyChanged -= OnCommandBarChanged;
            }

            UnwireButtons();
        }

        private void WireButtons()
        {
            UnwireButtons();

            if (moveButton != null) moveButton.onClick.AddListener(HandleMoveClicked);
            if (attackButton != null) attackButton.onClick.AddListener(HandleAttackClicked);
            if (endTurnButton != null) endTurnButton.onClick.AddListener(HandleEndTurnClicked);
        }

        private void UnwireButtons()
        {
            if (moveButton != null) moveButton.onClick.RemoveListener(HandleMoveClicked);
            if (attackButton != null) attackButton.onClick.RemoveListener(HandleAttackClicked);
            if (endTurnButton != null) endTurnButton.onClick.RemoveListener(HandleEndTurnClicked);
        }

        private void HandleMoveClicked()
        {
            battleViewModel?.EnterMoveMode();
            Refresh();
        }

        private void HandleAttackClicked()
        {
            battleViewModel?.EnterAttackMode();
            Refresh();
        }

        private void HandleEndTurnClicked()
        {
            battleViewModel?.EndTurn();
            Refresh();
        }

        private void OnBattleViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void OnCommandBarChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (commandBarViewModel == null)
            {
                SetInteractable(moveButton, false);
                SetInteractable(attackButton, false);
                SetInteractable(endTurnButton, false);
                return;
            }

            SetInteractable(moveButton, commandBarViewModel.CanMove);
            SetInteractable(attackButton, commandBarViewModel.CanBasicAttack);
            SetInteractable(endTurnButton, commandBarViewModel.CanEndTurn);

            if (moveButtonText != null) moveButtonText.text = commandBarViewModel.MoveButtonText;
            if (attackButtonText != null) attackButtonText.text = commandBarViewModel.AttackButtonText;
            if (endTurnButtonText != null) endTurnButtonText.text = commandBarViewModel.EndTurnButtonText;
            if (modeText != null) modeText.text = battleViewModel != null ? battleViewModel.InputMode.ToString() : string.Empty;
            if (errorText != null) errorText.text = battleViewModel != null ? battleViewModel.LastError : string.Empty;
        }

        private static void SetInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }
}
