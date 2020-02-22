using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Класс, описывающий контроллер GUI
/// </summary>
public class GUIController : MonoBehaviour 
{
    #region Vars

    /// <summary>
    /// Image иконки типа текущего оружия
    /// </summary>
    [SerializeField] [Tooltip("Image иконки типа текущего оружия")] private Image _weaponIcon;

    /// <summary>
    /// Панель иконки типа текущего оружия
    /// </summary>
    [SerializeField] [Tooltip("Панель иконки типа текущего оружия")] private GameObject _weaponIconPanel;

    /// <summary>
    /// Image иконки статуса перезарядки текущего оружия
    /// </summary>
    [SerializeField] [Tooltip("Image иконки статуса перезарядки текущего оружия")] private Image _weaponStatusIcon;

    /// <summary>
    /// Панель иконки статуса перезарядки текущего оружия
    /// </summary>
    [SerializeField] [Tooltip("Панель иконки статуса перезарядки текущего оружия")] private GameObject _weaponStatusIconPanel;

    /// <summary>
    /// Панель GameOver
    /// </summary>
    [SerializeField] [Tooltip("Панель GameOver")] private GameObject _gameOverPanel;

    /// <summary>
    /// Sprite иконки бесконечности
    /// </summary>
    [SerializeField] [Tooltip("Sprite иконки бесконечности")] private Sprite _infinityIcon;

    /// <summary>
    /// Sprite иконки прогресс бара перезарядки
    /// </summary>
    [SerializeField] [Tooltip("Sprite иконки прогресс бара перезарядки")] private Sprite _progressBarIcon;

    #endregion

    #region Functions

    /// <summary>
    /// Отобразить изменение параметров при смене оружия
    /// </summary>
    /// <param name="weaponIcon">Иконка оружия</param>
    /// <param name="needReload">Используется ли перезарядка у оружия</param>
    public void ChangeWeapon(Sprite weaponIcon, bool needReload)
    {
        _weaponIcon.sprite = weaponIcon;
        if(needReload)
        {
            _weaponStatusIcon.sprite = _progressBarIcon;
            _weaponStatusIcon.fillAmount = 0;
        }
        else
        {
            _weaponStatusIcon.sprite = _infinityIcon;
            _weaponStatusIcon.fillAmount = 1;
        }
    }

    /// <summary>
    /// Изменить прогресс у визуализации перезарядки
    /// </summary>
    /// <param name="progress">Текущий прогресс (от 0 до 1)</param>
    public void ChangeReloadProgress(float progress)
    {
        if (_weaponStatusIcon.sprite == _progressBarIcon)
        {
            if (progress > 1) progress = 1;
            else if (progress < 0) progress = 0;
            _weaponStatusIcon.fillAmount = progress;
        }
    }

    /// <summary>
    /// Изменить состояние GameOver
    /// </summary>
    /// <param name="gameOverState">Новое состояние GameOver</param>
    public void ChangeGameOverState(bool gameOverState)
    {
        _gameOverPanel.SetActive(gameOverState);
        _weaponIconPanel.gameObject.SetActive(!gameOverState);
        _weaponStatusIconPanel.gameObject.SetActive(!gameOverState);
    }

    void Update()
    {
        //Если игра началась и активное оружие использует перезарядку, то обновить состояние прогресса перезарядки
        if(GameLevelController.Instance.GameState == GameState.GameStarted)
        {
            if(PlayerController.Instance.ActiveWeapon.NeedReloadFlag)
            {
                ChangeReloadProgress((Time.time - PlayerController.Instance.ActiveWeapon.TimeOfLastShoot) / PlayerController.Instance.ActiveWeapon.ShootTimeout);
            }
        }
    }

    #endregion
}
