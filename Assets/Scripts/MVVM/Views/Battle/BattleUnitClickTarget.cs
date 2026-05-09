using UnityEngine;

namespace MVVM.Views.Battle
{
    public class BattleUnitClickTarget : MonoBehaviour
    {
        [SerializeField] private BattleUnitView battleUnitView;

        private void Awake()
        {
            if (battleUnitView == null)
            {
                battleUnitView = GetComponentInParent<BattleUnitView>();
            }
        }

        private void OnMouseDown()
        {
            battleUnitView?.Select();
        }
    }
}
