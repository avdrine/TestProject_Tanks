using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Класс, описывающий пул для противников
/// </summary>
public class PoolOfEnemies : Pool 
{
    #region Events

    /// <summary>
    /// Событие при отключении объекта пула
    /// </summary>
    public UnityEvent OnDisablePoolObject = new UnityEvent();

    /// <summary>
    /// Событие при включении объекта пула
    /// </summary>
    public UnityEvent OnActivatePoolObject = new UnityEvent();

    #endregion

    /// <summary>
    /// Функция, описывающая процесс инициализации объекта
    /// </summary>
    /// <param name="position">Позиция, в которой создается объект</param>
    public void InstantiateObject(Vector3 position)
    {
        //Создаем переменную под контроллер объекта
        EnemyController enemyController = null;

        //Если пул выключенных объектов пуст
        if (_poolOfDisabled.Count == 0)
        {
            //Создаем нового противника
            GameObject newItem = Instantiate(PoolObjectPrefab, transform);
            enemyController = newItem.GetComponent<EnemyController>();

            //Добавляем созданный объект в пул активных элементов
            _poolOfEnabled.Add(enemyController);

            //Подписываемся на событие отключение только что созданного объекта
            enemyController.ObjectDisabledEvent.AddListener(OnDisabledObj);
        }
        //Если в пуле выключенных объектов есть элементы
        else
        {
            //Переносим выключенный объект из пула неактивных элементов в пул активных
            _poolOfEnabled.Add(_poolOfDisabled[_poolOfDisabled.Count - 1]);
            _poolOfDisabled.RemoveAt(_poolOfDisabled.Count - 1);
            enemyController = (EnemyController)_poolOfEnabled[_poolOfEnabled.Count - 1];
        }

        //Устанавливаем позицию противника в указанную точку
        enemyController.transform.position = new Vector3(position.x, position.y, 0);

        //Включить объект
        _poolOfEnabled[_poolOfEnabled.Count - 1].Enable();
        OnActivatePoolObject.Invoke();
    }

    /// <summary>
    /// Функция, описывающая процесс отключения объекта и перемещение его в резерв
    /// </summary>
    /// <param name="_object">Объект, который необходимо отключить</param>
    public new void DisableObj(IPoolObject _object)
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если нашли искомый объект
            if (_poolOfEnabled[i] == _object)
            {
                //Отключить объект
                _poolOfEnabled[i].DisableObj();
                return;
            }
        }
    }

    /// <summary>
    /// Функция, описывающая происходящее при переходе объекта в отключенный режим.
    /// Необходимо эту функцию подписать на событие отключения в объектах пула.
    /// </summary>
    /// <param name="_object">Объект, который был только что отключен</param>
    public new void OnDisabledObj(IPoolObject _object)
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если нашли искомый объект
            if (_poolOfEnabled[i] == _object)
            {
                //Переносим объект из пула активных элементов в пул неактивных
                _poolOfDisabled.Add(_poolOfEnabled[i]);
                _poolOfEnabled.RemoveAt(i);
                OnDisablePoolObject.Invoke();
                return;
            }
        }
    }

    /// <summary>
    /// Функция, описывающая процесс отключения всех активных объектов и перемещение их в резерв
    /// </summary>
    public void DisableAllObjects()
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = _poolOfEnabled.Count - 1; i >= 0; i--)
        {
            //Отключить объект
            _poolOfEnabled[i].DisableObj();
        }
    }

    /// <summary>
    /// Получить количество активных объектов в пуле
    /// </summary>
    /// <returns></returns>
    public int GetActiveObjectsCount()
    {
        return _poolOfEnabled.Count;
    }
}
