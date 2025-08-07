using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Roulette Data", menuName = "Roulette/Roulette Data")]
public class RouletteData : ScriptableObject
{

    public List<SliceData> slots;
}