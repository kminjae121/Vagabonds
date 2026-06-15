using Unity.Behavior;

namespace Code.Enemies
{
    [BlackboardEnum]
    public enum EnemyState
    {
        IDLE, CHASE, ATTACK, HIT, DEAD
    }
}