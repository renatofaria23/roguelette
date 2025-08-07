using UnityEngine;

public enum SliceEffectType { Damage, Heal, Armor, Empty }

[CreateAssetMenu(menuName = "Roulette/Slice")]
public class SliceData : ScriptableObject
{
    public string sliceName;
    public SliceEffectType effect;
    public int power;
    public int weight = 1;
    public Color displayColor;
}