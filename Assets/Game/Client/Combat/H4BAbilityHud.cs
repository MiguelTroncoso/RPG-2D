using UnityEngine;
using UnityEngine.UI;

namespace Lumbre.Game.Client.Combat
{
    public sealed class H4BAbilityHud : MonoBehaviour
    {
        [SerializeField] private H4BPlayerAbilityController abilities;
        [SerializeField] private Text label;

        private void Awake()
        {
            abilities ??= FindFirstObjectByType<H4BPlayerAbilityController>();
            label ??= GetComponent<Text>();
        }

        private void Update()
        {
            if (abilities == null || label == null)
            {
                return;
            }

            var defense = abilities.IsDefenseActive
                ? $"DEF {abilities.DefenseRemaining:0.0}s"
                : $"DEF {abilities.DefenseCooldownRemaining:0.0}s";
            label.text = $"CALOR {abilities.CurrentHeat}/{abilities.MaxHeat}  |  {defense}  |  AOE {abilities.AreaHeatCost}";
        }
    }
}
