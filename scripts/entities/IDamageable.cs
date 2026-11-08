namespace BulletNieHeaven.Entities;

/// <summary>
/// Interfejs dla obiektów, które mogą otrzymywać obrażenia (gracz, wrogowie itp.).
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Zadaje obrażenia obiektowi.
    /// </summary>
    /// <param name="amount">Wartość zadanych obrażeń.</param>
    void TakeDamage(float amount);
}
