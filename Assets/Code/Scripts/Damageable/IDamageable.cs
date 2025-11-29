public interface IDamageable
{
    public void InflictDamage(float damage);
    
    public float GetCurrentHealth();
    
    public bool IsAlive();
    
    public bool IsDead();
}
