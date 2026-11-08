using Godot;

namespace BulletNieHeaven.Entities;

/// <summary>
/// Reprezentuje pocisk wystrzeliwany przez gracza lub broń.
/// Odpowiada za ruch, czas życia (zasięg) oraz detekcję kolizji z przeciwnikami (Area2D).
/// </summary>
public partial class Bullet : Area2D
{
    /// <summary>
    /// Prędkość lotu pocisku w pikselach na sekundę.
    /// </summary>
    [Export]
    public float Speed { get; set; } = 600.0f;

    /// <summary>
    /// Wartość obrażeń zadawanych przeciwnikowi przy trafieniu.
    /// </summary>
    [Export]
    public float Damage { get; set; } = 25.0f;

    /// <summary>
    /// Maksymalny czas życia pocisku w sekundach zanim zniknie z mapy.
    /// </summary>
    [Export]
    public float Lifetime { get; set; } = 2.0f;

    /// <summary>
    /// Znormalizowany wektor kierunku lotu pocisku.
    /// </summary>
    public Vector2 Direction { get; set; } = Vector2.Right;

    private float _timeAlive = 0.0f;

    public override void _Ready()
    {
        // Podpięcie sygnałów kolizji Area2D
        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;
    }

    /// <summary>
    /// Inicjalizuje parametry pocisku w momencie wystrzału.
    /// </summary>
    /// <param name="direction">Kierunek lotu pocisku.</param>
    /// <param name="speed">Opcjonalna prędkość lotu.</param>
    /// <param name="damage">Opcjonalna wartość obrażeń.</param>
    /// <param name="lifetime">Opcjonalny czas życia.</param>
    public void Initialize(Vector2 direction, float? speed = null, float? damage = null, float? lifetime = null)
    {
        Direction = direction.Normalized();
        if (speed.HasValue) Speed = speed.Value;
        if (damage.HasValue) Damage = damage.Value;
        if (lifetime.HasValue) Lifetime = lifetime.Value;

        Rotation = Direction.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Ruch pocisku
        Position += Direction * Speed * (float)delta;

        // Licznik czasu życia pocisku (zasięg czasowy)
        _timeAlive += (float)delta;
        if (_timeAlive >= Lifetime)
        {
            QueueFree();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        HandleHit(body);
    }

    private void OnAreaEntered(Area2D area)
    {
        HandleHit(area);
    }

    /// <summary>
    /// Sprawdza czy trafiony obiekt jest przeciwnikiem i zadaje obrażenia.
    /// </summary>
    /// <param name="target">Trafiony obiekt (Area2D lub Body2D).</param>
    private void HandleHit(Node target)
    {
        // Ignoruj gracza i inne pociski
        if (target is Bullet || target is Player || target.IsInGroup("player"))
        {
            return;
        }

        // Sprawdź czy obiekt należy do grupy wrogów
        bool isEnemy = target.IsInGroup("enemies") || target.IsInGroup("enemy");

        // Jeśli trafiony obiekt implementuje interfejs IDamageable
        if (target is IDamageable damageable)
        {
            damageable.TakeDamage(Damage);
            QueueFree();
            return;
        }

        // Sprawdź metodę TakeDamage przez odbicie/call w Godot
        if (target.HasMethod("TakeDamage"))
        {
            target.Call("TakeDamage", Damage);
            QueueFree();
            return;
        }

        // Jeżeli to wróg bez metody obrażeń, pocisk i tak znika po trafieniu
        if (isEnemy)
        {
            QueueFree();
        }
    }
}
