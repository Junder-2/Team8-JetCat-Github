public interface IDamageable {
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    void TakeDamage(int amount);
}

public interface IDamager {
    public int Damage { get; set; }
}
