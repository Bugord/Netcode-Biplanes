using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "Plane Data", menuName = "Plane/Data")]
public class PlaneData : ScriptableObject
{
    [SerializedDictionary("Angle", "Modifier")]
    public SerializedDictionary<Angle, Vector2> liftForceModifiers;
    
    [SerializedDictionary("Angle", "Modifier")]
    public SerializedDictionary<Angle, Vector2> engineForceModifiers;

    public float engineForce;
    
    public float liftForce;
}