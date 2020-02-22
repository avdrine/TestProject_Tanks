using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс, описывающий контроллер РПГ
/// </summary>
public class RocketLauncherController : WeaponController
{
    #region Vars

    /// <summary>
    /// Позиции для спавна ракет
    /// </summary>
    [SerializeField] [Tooltip("Позиции для спавна ракет")] private Transform[] _positionsToSpawnRockets;

    /// <summary>
    /// Незапущенные ракеты
    /// </summary>
    [SerializeField] [Tooltip("Незапущенные ракеты")] private RocketController[] _activeRocket;

    /// <summary>
    /// Пул ракет
    /// </summary>
    [SerializeField] [Tooltip("Пул ракет")] private PoolOfRockets _poolOfRockets;

    /// <summary>
    /// Пул взрывов
    /// </summary>
    [SerializeField] [Tooltip("Пул взрывов")] private PoolOfExplosions _poolOfExplosions;

    /// <summary>
    /// Корутина для выстрелов с таймаутом
    /// </summary>
    private IEnumerator _shootOnceC;

    #endregion

    #region Functions

    void OnEnable()
    {
        ReloadRocket();
    }

    /// <summary>
    /// Сделать разовый выстрел
    /// </summary>
    private new void ShootOnce()
    {
        //Если таймер выстрела прошел - сделать выстрел
        if (Time.time - TimeOfLastShoot > ShootTimeout)
        {
            if (_shootOnceC != null) StopCoroutine(_shootOnceC);
            _shootOnceC = ShootOnce_Courotine();
            StartCoroutine(_shootOnceC);
        }
    }

    /// <summary>
    /// Корутина разового выстрела, выключающая спрайты выстрелов из пушек по истечению времени
    /// </summary>
    private IEnumerator ShootOnce_Courotine()
    {
        //Запускаем ракеты
        for(int i = 0; i < _activeRocket.Length; i++)
        {
            if(_activeRocket[i] != null)
                _activeRocket[i].Activate(_bulletDamage);
            _activeRocket[i] = null;
        }
        
        //Фиксируем время выстрела
        TimeOfLastShoot = Time.time;

        //Ждем таймаут
        yield return new WaitForSeconds(ShootTimeout);

        //Перезаряжаем ракеты
        ReloadRocket();
    }

    /// <summary>
    /// В зависимости от типа пушки, произвести выстрел нужного типа
    /// </summary>
    public new void Fire()
    {
        if (weaponFireType == WeaponFireType.OneShoot) ShootOnce();
        else if (weaponFireType == WeaponFireType.ContinuousFire)
        {
            if (ContiniousFireC == null) StartFire();
            else StopFire();
        }
    }

    /// <summary>
    /// Перезарядить ракеты
    /// </summary>
    private void ReloadRocket()
    {
        for (int i = 0; i < _positionsToSpawnRockets.Length; i++)
        {
            _activeRocket[i] = _poolOfRockets.InstantiateObject(new RocketPoolObjectParameters(_bulletDamage, _positionsToSpawnRockets[i], _poolOfExplosions));
        }
    }

    /// <summary>
    /// Начать продолжительный огонь из пушки
    /// </summary>
    private new void StartFire()
    {
        if (ContiniousFireC != null) StopCoroutine(ContiniousFireC);
        ContiniousFireC = DoFire_Couroutine();
        StartCoroutine(ContiniousFireC);
    }

    /// <summary>
    /// Корутина ведения продолжительного огня
    /// </summary>
    private new IEnumerator DoFire_Couroutine()
    {
        while (true)
        {
            ShootOnce();
            yield return new WaitForSeconds(ShootTimeout);
        }
    }

    /// <summary>
    /// Остановить продолжительный огонь
    /// </summary>
    public new void StopFire()
    {
        if (ContiniousFireC != null)
            StopCoroutine(ContiniousFireC);
        ContiniousFireC = null;
    }

    #endregion
}
