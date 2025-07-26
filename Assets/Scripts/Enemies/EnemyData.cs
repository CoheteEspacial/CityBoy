using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float health = 10f;
    public float damage = 1f;
    public float contactDamage = 1f;
    public bool isFlying = false;

}
