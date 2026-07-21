using Lumbre.Game.Domain.Combat;
using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public sealed class UnityCombatTimeSource : ICombatTimeSource
    {
        public float Now => Time.time;
    }
}
