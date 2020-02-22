using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Класс, описывающий событие, которое передает контроллер пули
/// </summary>
public class ExplosionEvent : UnityEvent<ExplosionController> { };


/// <summary>
/// Структура, описывающая требования к параметрам, используемых для инициализации элементов пула 
/// </summary>
public struct ExplosionPoolObjectParameters : PoolObjectParameters 
{ 
    /// <summary>
    /// Урон
    /// </summary>
    public float damage; 

    /// <summary>
    /// Время жизни
    /// </summary>
    public float timeToLive; 

    /// <summary>
    /// Место спавна
    /// </summary>
    public Transform spawnTransform; 
    public ExplosionPoolObjectParameters(float newDamage, float newtimeToLive, Transform newSpawnTransform) { damage = newDamage; timeToLive = newtimeToLive; spawnTransform = newSpawnTransform; } 
}

/// <summary>
/// Класс, описывающий контроллер взрыва
/// </summary>
public class ExplosionController : MonoBehaviour, IPoolObject
{
    #region Vars

    /// <summary>
    /// Ссылка на Rigidbody2D
    /// </summary>
    [SerializeField] [Tooltip("Перенесите локальный компонент Rigidbody2D")] private Rigidbody2D _rb;

    /// <summary>
    /// Урон
    /// </summary>
    private float _damage;

    #endregion

    #region Events 

    /// <summary>
    /// Событие отключения объекта
    /// </summary>
    [HideInInspector] public ExplosionEvent ObjectDisabledEvent = new ExplosionEvent();

    #endregion


    #region Implementation of IPoolObject

    /// <summary>
    /// Функция для включения и инициализации объекта
    /// </summary>
    /// <param name="_parameters">Параметры, необходимые для инициализации (необязательно)</param>
    /// <returns>Успешность инициализации</returns>
    public bool Enable(PoolObjectParameters _parameters = null)
    {
        ExplosionPoolObjectParameters initializeParameters = (ExplosionPoolObjectParameters)_parameters;

        //Устанавливаем параметры инициализации
        _damage = initializeParameters.damage;
        transform.position = initializeParameters.spawnTransform.position;
        transform.rotation = initializeParameters.spawnTransform.rotation;

        //Сбросить ускорения rigidbody
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0;

        //Включаем объект
        gameObject.SetActive(true);

        //Включаем корутину для отключения по таймеру
        StartCoroutine(Activate_Couroutine(initializeParameters.timeToLive));

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

    /// <summary>
    /// Таймер для отключения по истечению заданного времени жизни
    /// </summary>
    /// <param name="timeToLive">Время жизни</param>
    private IEnumerator Activate_Couroutine(float timeToLive)
    {
        yield return new WaitForSeconds(timeToLive);
        DisableObj();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Если урон не нулевой
        if (_damage != 0)
        {
            //Нанести урон, если у объекта есть компонент жизней
            HealthController healthController = collision.gameObject.GetComponent<HealthController>();
            if (healthController != null) healthController.GetDamage(_damage);

            //Толкаем, если нужно
            CustomPhysicsController customPhysicsController = collision.gameObject.GetComponent<CustomPhysicsController>();
            if (customPhysicsController != null)
            {
                customPhysicsController.AddExplosionPush((collision.gameObject.transform.position - transform.position) * 4);
            }
        }
    }

    #endregion
}
