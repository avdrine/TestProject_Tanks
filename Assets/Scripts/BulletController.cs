using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Класс, описывающий событие, которое передает контроллер пули
/// </summary>
public class BulletEvent : UnityEvent<BulletController> { };

/// <summary>
/// Структура, описывающая требования к параметрам, используемых для инициализации элементов пула 
/// </summary>
public struct BulletPoolObjectParameters : PoolObjectParameters { public float damage; public Transform spawnTransform; public BulletPoolObjectParameters(float newDamage, Transform newSpawnTransform) { damage = newDamage; spawnTransform = newSpawnTransform; } }

/// <summary>
/// Класс, описывающий контроллер пули
/// </summary>
public class BulletController : MonoBehaviour, IPoolObject
{
    #region Vars
    /// <summary>
    /// Ссылка на Rigidbody2D
    /// </summary>
    [SerializeField] [Tooltip("Перенесите локальный компонент Rigidbody2D")] private Rigidbody2D _rb;

    /// <summary>
    /// Скорость полета пули
    /// </summary>
	[SerializeField] [Tooltip("Скорость полета пули")] private float _speed;

    /// <summary>
    /// Урон от пули
    /// </summary>
	[SerializeField] [Tooltip("Урон от пули")] private float _damage;
    #endregion

    #region Events

    //Событие отключения объекта
    [HideInInspector] public BulletEvent ObjectDisabledEvent = new BulletEvent();

    #endregion

    #region Implementation of IPoolObject
    /// <summary>
    /// Функция для включения и инициализации объекта
    /// </summary>
    /// <param name="_parameters">Параметры, необходимые для инициализации (необязательно)</param>
    /// <returns>Успешность инициализации</returns>
    public bool Enable(PoolObjectParameters _parameters = null)
    {
        BulletPoolObjectParameters initializeParameters = (BulletPoolObjectParameters)_parameters;
        
        //Устанавливаем параметры инициализации
        _damage = initializeParameters.damage;
        transform.position = initializeParameters.spawnTransform.position;
        transform.rotation = initializeParameters.spawnTransform.rotation;

        //Включаем объект
        gameObject.SetActive(true);

        //Сбросить ускорения rigidbody
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0;

        //Задаем импульс для полета
        _rb.AddRelativeForce(Vector2.up * _speed * _rb.mass);

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

    void OnCollisionEnter2D(Collision2D collision)
	{
        //Нанести урон, если у объекта есть компонент жизней
		HealthController healthController = collision.gameObject.GetComponent<HealthController>();
        if (healthController != null) healthController.GetDamage(_damage);

        //отключить объект
        DisableObj();
    }

    #endregion

}
