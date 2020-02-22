using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Интерфейс, описывающий требования к элементам пула
/// </summary>
public interface IPoolObject
{
    /// <summary>
    /// Функция для включения и инициализации объекта
    /// </summary>
    /// <param name="_parameters">Параметры, необходимые для инициализации (необязательно)</param>
    bool Enable(PoolObjectParameters _parameters = null);

    /// <summary>
    /// Функция для получения параметров объекта, которые используются для индексации элементов пула
    /// </summary>
    /// <returns>Параметры объекта, используемые для индексации</returns>
    PoolObjectParameters GetPoolObjectParameters();

    /// <summary>
    /// Получить текущее состояние активности (включен / выключен)
    /// </summary>
    /// <returns>Текущее состояние активности</returns>
    bool GetState();

    /// <summary>
    /// Функция, выключающая объект
    /// </summary>
    void DisableObj();
}

/// <summary>
/// Интерфейс для наследования, описывающий требования к параметрам, используемых для индексации элементов пула 
/// </summary>
public interface PoolObjectParameters { }
