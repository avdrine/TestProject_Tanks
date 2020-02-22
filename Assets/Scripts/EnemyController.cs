using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Класс, описывающий событие, которое передает контроллер противника
/// </summary>
public class EnemyEvent : UnityEvent<EnemyController> { };

/// <summary>
/// Класс, описывающий контроллер противника
/// </summary>
public class EnemyController : MonoBehaviour, IPoolObject
{
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

    #region Parameters

    /// <summary>
    /// Урон по игроку при столкновении
    /// </summary>
    [Header("Parameters")]
	[SerializeField] [Tooltip("Урон по игроку при столкновении")] private float _damage;

    /// <summary>
    /// Таймаут для нанесения урона
    /// </summary>
    [SerializeField] [Tooltip("Таймаут для нанесения урона")] private float _hitTimeoutOnStay;

    /// <summary>
    /// Цель для преследования
    /// </summary>
    [SerializeField] [Tooltip("Цель для преследования")] private Transform _target;

    /// <summary>
    /// Проставить игрока целью при старте
    /// </summary>
	[SerializeField] [Tooltip("Проставить игрока целью при старте")] private bool _playerIsTargetOnStart = true;

    /// <summary>
    /// Время последнего нанесения урона по игроку
    /// </summary>
    private float _lastHit;

    #endregion

#endregion

    #region Events
    /// <summary>
    /// Событие отключения объекта
    /// </summary>
    [HideInInspector] public EnemyEvent ObjectDisabledEvent = new EnemyEvent();

    #endregion

    #region Properties

    /// <summary>
    /// Цель для преследования
    /// </summary>
    public Transform Target { get { return _target; } set { _target = value; } }

    #endregion

    #region Implementation of IPoolObject
    /// <summary>
    /// Функция для включения и инициализации объекта
    /// </summary>
    /// <param name="_parameters">Параметры, необходимые для инициализации (необязательно)</param>
    /// <returns>Успешность инициализации</returns>
    public bool Enable(PoolObjectParameters _parameters = null)
    {
        _healthController.CurrHP = _healthController.MaxHP;

        //включаем объект
        gameObject.SetActive(true);

        //Проставляем цель для преследования, если преследуем игрока при активации противника
        if (_playerIsTargetOnStart && PlayerController.Instance != null)
        {
            _target = PlayerController.Instance.transform;
            _phC.Target = _target;
        }

        //Возвращаем успешный результат
        return true;
    }

    /// <summary>
    /// Функция для получения параметров объекта, которые используются для индексации элементов пула
    /// </summary>
    /// <returns>Параметры объекта, используемые для индексации</returns>
    public PoolObjectParameters GetPoolObjectParameters()
    {
        return null;
    }

    /// <summary>
    /// Получить текущее состояние активности (включен / выключен)
    /// </summary>
    /// <returns>Текущее состояние активности</returns>
    public bool GetState()
    {
        return gameObject.activeSelf;
    }

    /// <summary>
    /// Функция, выключающая объект
    /// </summary>
    public void DisableObj()
    {
        gameObject.SetActive(false);
        ObjectDisabledEvent.Invoke(this);
    }
    #endregion

    #region Functions

    void Update () 
	{
        //Двигаемся к цели, если она есть
        if(_target != null)
        {
            _phC.MoveBack = _target.transform.position.y < transform.position.y;
            _phC.MoveFront = _target.transform.position.y > transform.position.y;
            _phC.MoveLeft = _target.transform.position.x < transform.position.x;
            _phC.MoveRight = _target.transform.position.x > transform.position.x;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        //Наносим урон игроку, если касаемся его
        if (collision.gameObject.CompareTag("Player"))
        {
            DamageToPlayer(collision.gameObject);
        }
    }

	void OnCollisionEnter2D(Collision2D collision)
    {
        //Наносим урон игроку, если столкнулись с ним
        if (collision.gameObject.CompareTag("Player"))
		{
            DamageToPlayer(collision.gameObject);
		}
	}

    /// <summary>
    /// Нанесение урона игроку
    /// </summary>
    /// <param name="playerGO">GameObject игрока</param>
    private void DamageToPlayer(GameObject playerGO)
    {
        //Если урон ненулевой и прошел таймаут для нанесения урона
        if (_damage != 0 && Time.time - _lastHit > _hitTimeoutOnStay)
        {
            //Наносим урон
            HealthController healthController = playerGO.GetComponent<HealthController>();
            if (healthController != null) healthController.GetDamage(_damage);

            //Толкаем, если нужно
            CustomPhysicsController customPhysicsController = playerGO.GetComponent<CustomPhysicsController>();
            if (customPhysicsController != null)
            {
                if (customPhysicsController.PushThanTakeDamage)
                {
                    customPhysicsController.AddPushOnHit(playerGO.transform.position - transform.position);
                }
            }

            //Фиксируем время нанесения урона
            _lastHit = Time.time;
        }
    }

    #endregion
}
