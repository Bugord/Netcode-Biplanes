using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(menuName = "Plane/Flight Modifiers Data")]
    public class PlaneFlightModifiersData : ScriptableObject
    {
        [SerializedDictionary("Angle", "Modifier")]
        public SerializedDictionary<float, FlightModifier> engineForceModifiers;

        public FlightModifier GetModifiersForAngle(float angle)
        {
            if (angle is > 90f and <= 180f) {
                angle = 180f - angle;
            }

            if (angle is > 180f and <= 270f) {
                angle = 540f - angle;
            }

            if (engineForceModifiers.ContainsKey(angle)) {
                return engineForceModifiers[angle];
            }

            Debug.LogError($"[{nameof(PlaneFlightModifiersData)}] Modifiers for angle {angle} not found");
            return default;
        }
    }
}