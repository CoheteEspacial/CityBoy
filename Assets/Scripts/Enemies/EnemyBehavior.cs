using UnityEngine;

public abstract class EnemyBehavior : MonoBehaviour
{
    public abstract void Tick(); // Optional: for per-frame behavior
    public virtual void TryTrigger() { } // For conditional/one-shot actions
}
