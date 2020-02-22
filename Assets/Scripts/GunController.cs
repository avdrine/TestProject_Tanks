using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс, описывающий контроллер пушки
/// </summary>
public class GunController : WeaponController
{
    #region Vars

    /// <summary>
    /// Спрайты выстрелов из пушек
    /// </summary>
    [SerializeField] [Tooltip("Спрайты выстрелов из пушек")] private GameObject[] _fireShootSprite;

    /// <summary>
    /// Позиции для спавна пуль
    /// </summary>
    [SerializeField] [Tooltip("Позиции для спавна пуль")] private Transform[] _positionsToShoot;

    /// <summary>
    /// Время активности спрайтов выстрелов из пушек
    /// </summary>
	[SerializeField] [Tooltip("Время активности спрайтов выстрелов из пушек")] private float _timeToActivateFireShootSprite;

    /// <summary>
    /// Пул пуль
    /// </summary>
    [SerializeField] [Tooltip("Пул пуль")] private PoolOfBullets _poolOfBullets;

    /// <summary>
    /// Корутина для выстрелов с таймаутом
    /// </summary>
	private IEnumerator _shootOnceC;

    #endregion

    #region Functions

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
        //Создаем пули
        for (int i = 0; i < _positionsToShoot.Length; i++)
        {
            _poolOfBullets.InstantiateObject(new BulletPoolObjectParameters(_bulletDamage, _positionsToShoot[i]));
        }
		
        //Фиксируем время выстрела
        TimeOfLastShoot = Time.time;

        //Активируем спрайты выстрелов из пушек
        for (int i = 0; i < _fireShootSprite.Length; i++)
            _fireShootSprite[i].SetActive(true);

        //Ждем таймаут
		yield return new WaitForSeconds(_timeToActivateFireShootSprite);

        //Выключаем спрайты выстрелов из пушек
        for (int i = 0; i < _fireShootSprite.Length; i++)
            _fireShootSprite[i].SetActive(false);
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
        if(ContiniousFireC != null)
            StopCoroutine(ContiniousFireC); 
        ContiniousFireC = null;
        for (int i = 0; i < _fireShootSprite.Length; i++)
            _fireShootSprite[i].SetActive(false);
    }

    #endregion
}
