using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ps.Data.NodeServer.Info.Enums.Packets
{
    /// <summary>
    /// Пакеты которые принимает сервер
    /// </summary>
    public enum ServerPacketsEnum : ushort
    {
        #region Profile

        /// <summary>
        /// Отправка ключа шифрования
        /// </summary>
        Security = 1,

        /// <summary>
        /// Авторизация на аккаунте
        /// </summary>
        LogIn,

        /// <summary>
        /// Восстановление сессии (После разрыва)
        /// </summary>
        RecoverySession,

        /// <summary>
        /// Запрос данных о текущем аккаунте
        /// </summary>
        ProfileData,

        #endregion

        #region Data

        /// <summary>
        /// Запрос данных о доступных типах персонажей
        /// </summary>
        InfoData,

        /// <summary>
        /// Запрос данных конфигураций клиента
        /// </summary>
        ConfigData,

        /// <summary>
        /// Запрос данных о доступных вещах
        /// </summary>
        ItemData,

        /// <summary>
        /// Запрос данных о доступных картах
        /// </summary>
        MapData,

        #endregion

        #region Sandbox

        /// <summary>
        /// Запрос на установку статуса в рассылке комнат в песочнице
        /// </summary>
        SandboxReady,

        /// <summary>
        /// Запрос на создание комнаты в песочнице
        /// </summary>
        SandboxCreate,

        /// <summary>
        /// Запрос на подключение к комнате в песочнице
        /// </summary>
        SandboxConnect,

        /// <summary>
        /// Запрос на информацию о игроках в комнате песочницы
        /// </summary>
        SandboxUserInfoList,

        #endregion

        #region Shootout

        /// <summary>
        /// Запрос на создание комнаты на выбывание
        /// </summary>
        ShootoutCreate,

        /// <summary>
        /// Запрос на подключение к комнате на выбывание
        /// </summary>
        ShootoutConnect,

        /// <summary>
        /// Запрос на отключение от комнаты на выбывание
        /// </summary>
        ShootoutDisconnect,

        /// <summary>
        /// Запрос на установку статуса рассылки комнат на выбывание
        /// </summary>
        ShootoutReady,

        /// <summary>
        /// Запрос на информацию о игроках в комнате на выбывание
        /// </summary>
        ShootoutUserInfoList,

        /// <summary>
        /// Запрос на смену команды
        /// </summary>
        ShootoutChangeTeam,

        #endregion

        #region FastFight

        /// <summary>
        /// Запрос на установку статуса рассылки игроков в быстрых боях
        /// </summary>
        FastFightReady,

        /// <summary>
        /// Запрос на битву в быстром бою
        /// </summary>
        FastFightRequest,

        /// <summary>
        /// Запрос на поиск случайного противника в быстром бою
        /// </summary>
        FastFightRandomRequest,

        /// <summary>
        /// Ответ на вызов в быстром бою
        /// </summary>
        FastFightResponse,

        #endregion

        #region Tournament

        /// <summary>
        /// Запрос на список доступных турниров
        /// </summary>
        TournamentReady,

        /// <summary>
        /// Запрос на подключении к турниру
        /// </summary>
        TournamentConnect,

        #endregion

        #region Character

        /// <summary>
        /// Запрос на создания персонажа
        /// </summary>
        CharacterCreate,

        /// <summary>
        /// Запрос на установку основного персонажа
        /// </summary>
        CharacterEquip,

        /// <summary>
        /// Запрос на список персонажа на текущем аккаунте
        /// </summary>
        CharacterList,

        /// <summary>
        /// Запрос на удаление персонажа с текущего аккаунта
        /// </summary>
        CharacterRemove,

        #endregion

        #region Item

        /// <summary>
        /// Запрос на одевание вещи
        /// </summary>
        ItemEquip,

        /// <summary>
        /// Запрос на список вещей в инвентаре
        /// </summary>
        ItemInventoryList,

        /// <summary>
        /// Запрос на продажу вещи с инвентаря
        /// </summary>
        ItemSell,

        /// <summary>
        /// Запрос на улучшение вещи в инвентаре
        /// </summary>
        ItemUpgrade,

        /// <summary>
        /// Запрос на восстановление вещи в инвентаре
        /// </summary>
        ItemRepair,

        /// <summary>
        /// Запрос на проверку данных вещи 
        /// </summary>
        ItemInventoryCheck,

        #endregion

        #region Shop

        /// <summary>
        /// Запрос на список вещей в магазине
        /// </summary>
        ShopItemList,

        /// <summary>
        /// Запрос на покупку вещи в магазине
        /// </summary>
        ShopBuyCharacterCell,

        /// <summary>
        /// Запрос на покупку серебра в магазине
        /// </summary>
        ShopBuyOunce,
        ShopBuyPremium,
        ShopBuyItem,

        #endregion

        ConsoleMessage,

        SandboxDisconnect,
        LevelData,

        CharacterStatistics,
        ItemStatusBoost,
        ShootoutBattleReadyResult,
    }
}
