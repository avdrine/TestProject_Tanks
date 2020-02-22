﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс, описывающий пул для ракет
/// </summary>
public class PoolOfRockets : Pool
{ 
    /// <summary>
    /// Функция, описывающая процесс инициализации объекта
    /// </summary>
    /// <param name="_createParameter">Параметры создания</param>
    public new RocketController InstantiateObject(PoolObjectParameters _createParameter)
    {
        //Создаем переменную под контроллер объекта
        RocketController rocketController = null;

        //Если пул выключенных объектов пуст
        if (_poolOfDisabled.Count == 0)
        {
            //Создаем новую пулю
            GameObject newItem = Instantiate(PoolObjectPrefab, transform);
            rocketController = newItem.GetComponent<RocketController>();

            //Добавляем созданный объект в пул активных элементов
            _poolOfEnabled.Add(rocketController);

            //Подписываемся на событие отключение только что созданного объекта
            rocketController.ObjectDisabledEvent.AddListener(OnDisabledObj);
        }
        //Если в пуле выключенных объектов есть элементы
        else
        {
            //Переносим выключенный объект из пула неактивных элементов в пул активных
            _poolOfEnabled.Add(_poolOfDisabled[_poolOfDisabled.Count - 1]);
            _poolOfDisabled.RemoveAt(_poolOfDisabled.Count - 1);
            rocketController = (RocketController)_poolOfEnabled[_poolOfEnabled.Count - 1];
        }
        //Включить объект
        _poolOfEnabled[_poolOfEnabled.Count - 1].Enable(_createParameter);

        return rocketController;
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
                return;
            }
        }
    }
}
