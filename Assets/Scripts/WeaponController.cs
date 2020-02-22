using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Перечисление типов ведения огня
/// </summary>
public enum WeaponFireType { OneShoot, ContinuousFire }

/// <summary>
/// Абстрактный класс, описывающий поведение оружия
/// </summary>
public abstract class WeaponController : MonoBehaviour
{
    #region Vars

    /// <summary>
    /// Префаб пули
    /// </summary>
    [SerializeField] [Tooltip("Префаб пули")] protected GameObject _bulletPrefab;

    /// <summary>
    /// Урон пули
    /// </summary>
    [SerializeField] [Tooltip("Урон пули")] protected float _bulletDamage;

    /// <summary>
    /// Таймаут выстрела
    /// </summary>
    [SerializeField] [Tooltip("Таймаут выстрела")] private float shootTimeout;

    /// <summary>
    /// Время последнего выстрела
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Время последнего выстрела")] private float timeOfLastShoot;

    /// <summary>
    /// Тип ведения огня
    /// </summary>
    [SerializeField] [Tooltip("Тип ведения огня")] protected WeaponFireType weaponFireType;

    /// <summary>
    /// Флаг активности оружия у носителя
    /// </summary>
    [SerializeField] [Tooltip("Флаг активности оружия у носителя")] private bool isActiveWeapon;

    /// <summary>
    /// Иконка для GUI
    /// </summary>
    [SerializeField] [Tooltip("Иконка для GUI")] private Sprite _GUIIcon;

    /// <summary>
    /// Требуется ли перезарядка для оружия (параметр для GUI)
    /// </summary>
    [SerializeField] [Tooltip("Требуется ли перезарядка для оружия (параметр для GUI)")] private bool _needReloadFlag;

    /// <summary>
    /// Корутина продолжительного огня
    /// </summary>
    protected IEnumerator ContiniousFireC;

    #endregion

    #region Properties

    /// <summary>
    /// Флаг активности оружия у носителя
    /// </summary>
    public bool IsActiveWeapon { get { return isActiveWeapon; } set { isActiveWeapon = value; } }

    /// <summary>
    /// Время последнего выстрела
    /// </summary>
    public float TimeOfLastShoot { get { return timeOfLastShoot; } protected set { timeOfLastShoot = value; } }

    /// <summary>
    /// Таймаут выстрела
    /// </summary>
    public float ShootTimeout { get { return shootTimeout; } protected set { shootTimeout = value; } }

    /// <summary>
    /// Требуется ли перезарядка для оружия (параметр для GUI)
    /// </summary>
    public bool NeedReloadFlag { get { return _needReloadFlag; } private set { _needReloadFlag = value; } }

    /// <summary>
    /// Иконка для GUI
    /// </summary>
    public Sprite GUIIcon { get { return _GUIIcon; } set { _GUIIcon = value; } }

    #endregion

    #region Functions

    /// <summary>
    /// В зависимости от типа пушки, произвести выстрел нужного типа
    /// </summary>
    public virtual void Fire() 
	{
		if (weaponFireType == WeaponFireType.OneShoot) ShootOnce();
		else if(weaponFireType == WeaponFireType.ContinuousFire)
		{
			if (ContiniousFireC == null) StartFire();
			else StopFire();
		}
	}

    /// <summary>
    /// Начать продолжительный огонь из пушки
    /// </summary>
	protected virtual void StartFire() 
	{
		if (ContiniousFireC != null) StopCoroutine(ContiniousFireC);
		ContiniousFireC = DoFire_Couroutine();
		StartCoroutine(ContiniousFireC);
	}

    /// <summary>
    /// Корутина ведения продолжительного огня
    /// </summary>
	protected virtual IEnumerator DoFire_Couroutine()
	{
		while(true)
		{
			ShootOnce();
			yield return new WaitForSeconds(ShootTimeout);
		}
	}

    /// <summary>
    /// Остановить продолжительный огонь
    /// </summary>
	public virtual void StopFire() 
	{
		StopCoroutine(ContiniousFireC);
	}

    /// <summary>
    /// Сделать разовый выстрел
    /// </summary>
	protected virtual void ShootOnce() { print("ShootOnce"); }

    #endregion
}
