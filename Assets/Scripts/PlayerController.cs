using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс, описывающий контроллер игрока
/// </summary>
public class PlayerController : MonoBehaviour 
{
    /// <summary>
    /// Singleton
    /// </summary>
    public static PlayerController Instance;

    #region Vars

    #region Components Links

    /// <summary>
    /// Ссылка на CustomPhysicsController
    /// </summary>
    [Header("Components Links")]
    [SerializeField] [Tooltip("Перенесите локальный компонент CustomPhysicsController")] private CustomPhysicsController _phC;

    /// <summary>
    /// Ссылка на HealthController
    /// </summary>
    [SerializeField] [Tooltip("Перенесите локальный компонент HealthController")] private HealthController _healthController;

    #endregion

    #region Weapons parameters

    /// <summary>
    /// Активное оружие
    /// </summary>
    [Header("Weapons parameters")]
    [SerializeField] [Tooltip("Активное оружие")] private WeaponController _activeWeapon;

    /// <summary>
    /// Все доступные оружия
    /// </summary>
	[SerializeField] [Tooltip("Все доступные оружия")] private WeaponController[] _weapons;

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Активное оружие
    /// </summary>
    public WeaponController ActiveWeapon { get { return _activeWeapon; } private set { _activeWeapon = value; } }

    #endregion

    #region Functions

    void Awake()
    {
        //Singleton
        Instance = this;
    }

	void Update () 
    {
		CheckInput();
	}

    void OnDisable()
    {
        //При выключении отключить огонь из всех орудий
        for(int i = 0; i < _weapons.Length; i++)
        {
            switch (_weapons[i].GetType().Name)
            {
                case "RocketLauncherController": ((RocketLauncherController)_weapons[i]).StopFire(); break;
                case "GunController": ((GunController)_weapons[i]).StopFire(); break;
            }
        }
    }

    /// <summary>
    /// Заспавнить игрока
    /// </summary>
    public void Spawn()
    {
        _phC.RigidbodyReinitialize();
        transform.position = Vector3.zero;
        transform.rotation = new Quaternion();
        _healthController.CurrHP = _healthController.MaxHP;
        GameLevelController.Instance.GUIController.ChangeWeapon(ActiveWeapon.GUIIcon, ActiveWeapon.NeedReloadFlag);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Проверить ввод
    /// </summary>
	private void CheckInput()
	{
        _phC.MoveBack = Input.GetKey(KeyCode.DownArrow);
        _phC.MoveFront = Input.GetKey(KeyCode.UpArrow);
        _phC.MoveLeft = Input.GetKey(KeyCode.LeftArrow);
        _phC.MoveRight = Input.GetKey(KeyCode.RightArrow);

        //Если нажата X - огонь
        if (Input.GetKeyDown(KeyCode.X))
        {
            switch (ActiveWeapon.GetType().Name)
            {
                case "RocketLauncherController": ((RocketLauncherController)ActiveWeapon).Fire(); break;
                case "GunController": ((GunController)ActiveWeapon).Fire(); break;
            }
        }

        //Если нажата Q - сменить оружие на предыдущее
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //Остановить огонь из текущего оружия
            switch (ActiveWeapon.GetType().Name)
            {
                case "RocketLauncherController": ((RocketLauncherController)ActiveWeapon).StopFire(); break;
                case "GunController": ((GunController)ActiveWeapon).StopFire(); break;
            }

            //Изменить текущее оружие
            for (int i = 0; i < _weapons.Length; i++)
            {
                _weapons[i].IsActiveWeapon = false;
                if (_weapons[i] == ActiveWeapon)
                {
                    i--; 
                    if (i < 0) ActiveWeapon = _weapons[_weapons.Length - 1];
                    else ActiveWeapon = _weapons[i];
                    ActiveWeapon.IsActiveWeapon = true;
                    break;
                }
            }
            ActiveWeapon.IsActiveWeapon = true;

            //Обновить GUI
            GameLevelController.Instance.GUIController.ChangeWeapon(ActiveWeapon.GUIIcon, ActiveWeapon.NeedReloadFlag);
        }

        //Если нажата W - сменить оружие на следующее
        if (Input.GetKeyDown(KeyCode.W))
        {
            //Остановить огонь из текущего оружия
            switch (ActiveWeapon.GetType().Name)
            {
                case "RocketLauncherController": ((RocketLauncherController)ActiveWeapon).StopFire(); break;
                case "GunController": ((GunController)ActiveWeapon).StopFire(); break;
            }

            //Изменить текущее оружие
            for (int i = 0; i < _weapons.Length; i++)
            {
                _weapons[i].IsActiveWeapon = false;
                if (_weapons[i] == ActiveWeapon)
                {
                    i++; 
                    if (i > _weapons.Length - 1) ActiveWeapon = _weapons[0];
                    else ActiveWeapon = _weapons[i];
                    break;
                }
            }
            ActiveWeapon.IsActiveWeapon = true;

            //Обновить GUI
            GameLevelController.Instance.GUIController.ChangeWeapon(ActiveWeapon.GUIIcon, ActiveWeapon.NeedReloadFlag);
        }

    }

    #endregion
}
