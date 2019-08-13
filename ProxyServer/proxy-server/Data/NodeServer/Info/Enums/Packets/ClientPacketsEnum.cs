using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ps.Data.NodeServer.Info.Enums.Packets
{
    /// <summary>
    /// Пакеты которые принимает клиент
    /// </summary>
    public enum ClientPacketsEnum : ushort
    {
        #region Profile

        /// <summary>
        /// Результат отправки ключа шифрования
        /// </summary>
        SecurityResult = 1,

        /// <summary>
        /// Результат авторизации аккаунта
        /// </summary>
        LogInResult,

        /// <summary>
        /// Вход на текущий аккаунт с другого компьютера (разрыв сессии)
        /// </summary>
        AnotherAuthMessage,

        /// <summary>
        /// Результат восстановления сессии(после разрыва соединения)
        /// </summary>
        RecoverySessionResult,

        /// <summary>
        /// Результат запроса данных о текущем аккаунте
        /// </summary>
        ProfileDataResult,

        #endregion

        #region Data

        /// <summary>
        /// Результат запроса данных о доступных типах персонажей
        /// </summary>
        InfoDataResult,

        /// <summary>
        /// Результат запроса данных о доступных данных конфигурации клиента
        /// </summary>
        ConfigDataResult,

        /// <summary>
        /// Результат запроса данных о доступных вещах
        /// </summary>
        ItemDataResult,

        /// <summary>
        /// Результат запроса данных о доступных картах
        /// </summary>
        MapDataResult,

        #endregion

        #region Sandbox

        /// <summary>
        /// Результат запроса на создание комнаты в песочнице
        /// </summary>
        SandboxCreateResult,

        /// <summary>
        /// Результат запроса на подключение к комнате
        /// </summary>
        SandboxConnectResult,

        /// <summary>
        /// Сообщение о отключении игрока с комнаты песочницы
        /// </summary>
        SandboxDisconnect,

        /// <summary>
        /// Результат запроса на установку статуса рассылки комнат в песочнице (список комнат)
        /// </summary>
        SandboxRoomListResult,

        /// <summary>
        /// Результат запроса на информацию о игроках в комнате песочницы
        /// </summary>
        SandboxUserInfoListResult,

        /// <summary>
        /// Сообщение о создании комнаты в песочнице
        /// </summary>
        SandboxRoomCreated,

        /// <summary>
        /// Сообщение о удалении комнаты в песочнице
        /// </summary>
        SandboxRoomRemoved,

        /// <summary>
        /// Сообщение о изменении кол-ва игроков комнаты песочницы
        /// </summary>
        SandboxChangePlayerCount,

        #endregion

        #region Shootout

        /// <summary>
        /// Результат запроса на создании комнаты на выбывание
        /// </summary>
        ShootoutCreateResult,

        /// <summary>
        /// Результат запроса на подключении к комнате на выбывание
        /// </summary>
        ShootoutConnectResult,

        /// <summary>
        /// Результат запроса на установку статуса рассылки комнат на выбывание (список комнат)
        /// </summary>
        ShootoutRoomListResult,

        /// <summary>
        /// Сообщение о создании комнаты
        /// </summary>
        ShootoutRoomCreated,

        /// <summary>
        /// Сообщение о удалении комнаты
        /// </summary>
        ShootoutRoomRemoved,

        /// <summary>
        /// Сообщение о подключении игрока в текущую комнату на выбывание
        /// </summary>
        ShootoutPlayerConnected,

        /// <summary>
        /// Сообщение о отключении игрока в текущую комнату на выбывание
        /// </summary>
        ShootoutPlayerDisconnected,

        /// <summary>
        /// Сообщение о смене комманды игрока в текущей комнате на выбывание
        /// </summary>
        ShootoutChangeTeam,

        /// <summary>
        /// Сообщение о изменении кол-ва игроков комнаты на выбывание
        /// </summary>
        ShootoutChangePlayerCount,

        /// <summary>
        /// Результат запроса на информацию о игроках в комнате на выбывание
        /// </summary>
        ShootoutUserInfoListResult,

        #endregion

        #region FastFight

        /// <summary>
        /// Результат запроса на установку статуса рассылки игроков в быстрых боях (список список игроков)
        /// </summary>
        FastFightReadyList,

        /// <summary>
        /// Получение запроса на быструю битву от другого игрока
        /// </summary>
        FastFightRequest,

        /// <summary>
        /// Результат запроса на быструю битву
        /// </summary>
        FastFightRequestResult,

        /// <summary>
        /// Сообщение о подключении игрока к лобби быстрого боя
        /// </summary>
        FastFightPlayerConnected,

        /// <summary>
        /// Сообщение о отключении игрока от лобби быстрого боя
        /// </summary>
        FastFightPlayerDisconnected,

        #endregion

        #region Tournament

        /// <summary>
        /// Результат запроса на список доступных турниров
        /// </summary>
        TournamentListResult,

        /// <summary>
        /// Результат запроса на подключении к турниру
        /// </summary>
        TournamentConnectResult,

        #endregion

        #region Character

        /// <summary>
        /// Результат запроса на создание персонажа
        /// </summary>
        CharacterCreateResult,

        /// <summary>
        /// Результат запроса на установку основного персонажа
        /// </summary>
        CharacterEquipResult,

        /// <summary>
        /// Результат запроса на список персонажей на текущем аккаунте
        /// </summary>
        CharacterListResult,

        /// <summary>
        /// Результат запроса на удаление персонажа с текущего аккаунта
        /// </summary>
        CharacterRemoveResult,

        #endregion

        #region Item

        /// <summary>
        /// Результат запроса на одевание вещи
        /// </summary>
        ItemEquipResult,

        /// <summary>
        /// Результат запроса на список вещей в инвентаре
        /// </summary>
        ItemInventoryListResult,

        /// <summary>
        /// Результат запроса на продажу вещи в инвентаре
        /// </summary>
        ItemSellResult,

        /// <summary>
        /// Результат запроса на улучшение вещи в инвентаре
        /// </summary>
        ItemUpgradeResult,

        /// <summary>
        /// Результат запроса на восстановление вещи в инвентаре
        /// </summary>
        ItemRepairResult,

        /// <summary>
        /// Сообщение с данные на новую вещь в инвентаре
        /// </summary>
        InventoryItem,

        #endregion

        #region Shop

        /// <summary>
        /// Результат запроса на список вещей в магазине
        /// </summary>
        ShopItemListResult,

        /// <summary>
        /// Результат запроса на покупку вещи в магазине
        /// </summary>
        ShopBuyItemResult,

        /// <summary>
        /// Результат запроса на покупку серебра в магазине
        /// </summary>
        ShopBuyOunceResult,

        #endregion

        #region Messages

        /// <summary>
        /// Сообщение о том что сервер занят
        /// </summary>
        NodeHostClientBusyResult,

        /// <summary>
        /// Информация для подключения к комнате
        /// </summary>
        RoomConnectionInfoMessage,

        /// <summary>
        /// Сообщение с новыми данными валюты
        /// </summary>
        UpdateCoinsMessage,
        CharacterChangeStates,
        ShopBuyCharacterCellResult,
        ShopBuyPremiumResult,

        #endregion

        TournamentRoomListResult,
        TournamentRoomPlayerDisconnected,
        TournamentRoomPlayerConnected,
        BattleResult,
        CharacterChangeLevel,
        FastFightRandomRequestResult,
        FastFightChangeStatus,
        ConsoleMessageResult,
        Refresh,
        LevelDataResult,
        CharacterStatisticsMessage,
        ItemStatusBoostResult,
        ShootoutBattleReady,
    }
}
