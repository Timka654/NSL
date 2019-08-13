using BinarySerializer;
using BinarySerializer.Builder;
using BinarySerializer.DefaultTypes;
using phs.Data.NodeHostServer.Info;
using phs.Data.GameServer.Info;
using phs.Data.GameServer.Info.Enums;
using phs.Data.GameServer.Network;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Helper.Configuration;
using Utils.Helper.Configuration.Info;

namespace phs.Data.NodeHostServer.Network
{
    public class Structorian
    {
        public static void BuildStructures(TypeStorage storage)
        {
            //BuildConfigurationInfoStructure(storage);

            //BuildProfileInfoStructure(storage);

            //BuildLevelInfoStructure(storage);

            //BuildMapInfoStructure(storage);

            //BuildShopItemInfoStructure(storage);

            //BuildLobbyRoomInfoStructure<LobbyRoomInfo>(storage).Compile();

            //BuildNetworkNodeHostClientDataStructure(storage);

            //BuildItemInfoStructure(storage);
            //BuildItemEnhanceInfoStructure(storage);

            //BuildInventoryItemInfoStructure(storage);

            //BuildCharacterTypeInfoStructure(storage);
            //BuildCharacterStatisticsInfoStructure(storage);
            //BuildCharacterInfoStructure(storage);

            //BuildSandboxRoomInfoStructure(storage);
            //BuildShootoutRoomInfoStructure(storage);
            //BuildTournamentRoomInfoStructure(storage);
            //BuildLobbyPlayerInfoStructure<NodePlayerInfo>(storage).Compile();
            //BuildSandboxPlayerInfoStructure(storage);
            //BuildFastFightPlayerInfoStructure(storage);
            //BuildShootoutPlayerInfoStructure(storage);
            //BuildTournamentPlayerInfoStructure(storage);
            //var ts = BinarySerializer.TypeStorage.Instance;
        }

//        private static void BuildConfigurationInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<ConfigurationInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Name)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Value)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()
//                .Compile();
//        }

//        private static void BuildCharacterInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<CharacterInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Id)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "ExpChangeInfo", "StateChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.UserId)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("CharacterPublicInfo")
//                .SaveProperty()

//                .GetProperty(x => x.CharacterTypeId)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Name)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Level)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "ExpChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Exp)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "ExpChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.HackDamageMin)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "StateChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.HackDamageMax)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "StateChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.ChopDamageMin)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "StateChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.ChopDamageMax)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "StateChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Defense)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "StateChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.RunningSpeed)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "StateChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.AttackSpeed)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "StateChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Hp)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo", "StateChangeInfo")
//                .SaveProperty()

//                .GetProperty(x => x.EquipedItemList)
//                .SetBinaryType<BinaryList16<InventoryItemInfo>>()
//                .SetSchemes("RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.StatisticsMap)
//                .SetBinaryType<BinaryDictionary16<BinaryByte, CharacterStatisticsInfo>>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo")
//                .SaveProperty()
//                .Compile();
//        }

//        private static void BuildCharacterStatisticsInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<CharacterStatisticsInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Mode)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.TotalCount)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.WinsCount)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.WinsCount)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.DefeatsCount)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "CharacterPublicInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .Compile();
//        }

//        private static void BuildCharacterTypeInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<CharacterTypeInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Id)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Name)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Sex)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.IconMeshName)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Model3DMeshName)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Hp)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.HackDamageMin)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.HackDamageMax)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.ChopDamageMin)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.ChopDamageMax)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Defense)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.AttackSpeed)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.RunningSpeed)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.BodyType)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Race)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .Compile();
//        }

//        private static void BuildInventoryItemInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<InventoryItemInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Id)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("RoomPlayerInfo", "ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.CharacterId)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.ItemId)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Level)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomPlayerInfo", "ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Durability)
//                .SetBinaryType<BinaryFloat32>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.MaxDurability)
//                .SetBinaryType<BinaryFloat32>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.UpgradeTime)
//                .SetBinaryType<BinaryNullable<BinaryDateTime, DateTime>>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.State)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .Compile();
//        }

//        private static void BuildItemEnhanceInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<ItemEnhanceInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Level)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.HackDamageMin)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.HackDamageMax)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.ChopDamageMin)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.ChopDamageMax)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Defense)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.AttackSpeed)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.RunningSpeed)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.DurationTime)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.PriceOunce)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.PriceDenarius)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()
//                .Compile();
//        }

//        private static void BuildItemInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<ItemInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Id)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Name)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Description)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.ItemType)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.WearType)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Level)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")

//                .SaveProperty()
//                .GetProperty(x => x.PriceOunce)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.PriceDenarius)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.IconMeshPath)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Model3DMeshPath)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.MinCharacterLevel)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.MaxCharacterLevel)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Gender)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.EnhanceList)
//                .SetBinaryType<BinaryList16<ItemEnhanceInfo>>()
//                .SetSchemes("InfoData")
//                .SaveProperty()
//                .Compile();
//        }

//        private static void BuildLevelInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<LevelInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Level)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Exp)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .Compile();
//        }

//        private static void BuildMapInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<MapInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Id)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Mode)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Name)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.MeshPath)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.IconPath)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .Compile();

//        }

//        private static void BuildShopItemInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<ShopItemInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Id)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.Name)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.ItemId)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.PriceOunce)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .GetProperty(x => x.PriceDenarius)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("InfoData")
//                .SaveProperty()

//                .Compile();
//        }

//        private static void BuildSandboxPlayerInfoStructure(TypeStorage storage)
//        {
//            BuildLobbyPlayerInfoStructure<SandboxPlayerInfo>(storage)
//                .Compile();
//        }

//        private static void BuildFastFightPlayerInfoStructure(TypeStorage storage)
//        {
//            BuildLobbyPlayerInfoStructure<FastFightPlayerInfo>(storage)
//                .Compile();
//        }

//        private static void BuildShootoutPlayerInfoStructure(TypeStorage storage)
//        {
//            BuildLobbyPlayerInfoStructure<ShootoutPlayerInfo>(storage)
//                .Compile();
//        }

//        private static void BuildTournamentPlayerInfoStructure(TypeStorage storage)
//        {
//            BuildLobbyPlayerInfoStructure<TournamentPlayerInfo>(storage)
//                .Compile();
//        }

//        private static void BuildNetworkNodeHostClientDataStructure(TypeStorage storage)
//        {
//            StructBuilder<NetworkNodeHostClientData>.GetStruct(storage)
//                .SetSchemes("RoomConnectionInfo")

//                    .GetProperty(x => x.ServerData)
//                    .SetPartialType<NodeHostClientInfo>()
//                    .SetSchemes("RoomConnectionInfo")
//                        .GetProperty(x => x.IpAddr)
//                        .SetBinaryType<BinaryString16>()
//                        .SaveProperty()
//                        .GetProperty(x => x.Port)
//                        .SetBinaryType<BinaryInt32>()
//                        .SaveProperty()

//                    .SavePartialType()
//                    .SaveProperty()
//                    .Compile();

//        }

//        private static StructBuilder<T> BuildLobbyPlayerInfoStructure<T>(TypeStorage storage)
//            where T : NodePlayerInfo
//        {
//            return StructBuilder<T>
//                .GetStruct(storage)

//                .GetProperty(x => x.User)
//                .SetPartialType<ProfileInfo>()
//                .Back()
//                .SetSchemes("RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.CurrentCharacter)
//                .SetPartialType<CharacterInfo>()
//                .Back()
//                .SetSchemes("RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.TeamId)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.ConnectToken)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("RoomConnectionInfo")
//                .SaveProperty()

//                .GetProperty(x => x.CurrentRoom)
//                .SetPartialType<LobbyRoomInfo>()
//                .SavePartialType()
//                .SetSchemes("RoomConnectionInfo")
//                .SaveProperty()
//                ;
//        }

//        private static void BuildSandboxRoomInfoStructure(TypeStorage storage)
//        {
//            BuildLobbyRoomInfoStructure<SandboxRoomInfo>(storage)
                
//                .GetProperty(x => x.PlayTime)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .GetProperty(x => x.WaitTime)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .Compile();
//        }

//        private static void BuildShootoutRoomInfoStructure(TypeStorage storage)
//        {
//            BuildLobbyRoomInfoStructure<ShootoutRoomInfo>(storage)

//                .GetProperty(x => x.PlayTime)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .GetProperty(x => x.WaitTime)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .Compile();
//        }

//        private static void BuildTournamentRoomInfoStructure(TypeStorage storage)
//        {
//            BuildLobbyRoomInfoStructure<TournamentRoomInfo>(storage)

//                .GetProperty(x => x.RunTime)
//                .SetBinaryType<BinaryDateTime>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .Compile();
//        }

//        private static StructBuilder<T> BuildLobbyRoomInfoStructure<T>(TypeStorage storage)
//            where T : LobbyRoomInfo
//        {

//            return StructBuilder<T>
//                .GetStruct(storage)
//                .GetProperty(x => x.StartTime)
//                .SetBinaryType<BinaryNullable<BinaryDateTime, DateTime>>()
//                .SetSchemes("RoomConnectionInfo")
//                .SaveProperty()

//                .GetProperty(x => x.EndTime)
//                .SetBinaryType<BinaryNullable<BinaryDateTime, DateTime>>()
//                .SetSchemes("RoomConnectionInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Id)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("RoomInfo", "RoomConnectionInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Mode)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomConnectionInfo")
//                .SaveProperty()

//                .GetProperty(x => x.MapId)
//                .SetBinaryType<BinaryInt16>()
//                .SetSchemes("RoomInfo", "RoomConnectionInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Name)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .GetProperty(x => x.PlayerCount)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .GetProperty(x => x.MaxPlayerCount)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .GetProperty(x => x.CreateTime)
//                .SetBinaryType<BinaryDateTime>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .GetProperty(x => x.MinLevel)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .GetProperty(x => x.MaxLevel)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .GetProperty(x => x.PasswordEnabled)
//                .SetBinaryType<BinaryBool>()
//                .SetSchemes("RoomInfo")
//                .SaveProperty()

//                .GetProperty(x => x.PreConnectedList)
//                .SetBinaryType<BinaryList16<NodePlayerInfo>>()
//                .SetSchemes("RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.ConnectedList)
//                .SetBinaryType<BinaryList16<NodePlayerInfo>>()
//                .SetSchemes("RoomPlayerInfo")
//                .SaveProperty()

//                .SetSchemes("RoomConnectionInfo")

//                .GetProperty(x => x.GameServer)
//                .SetPartialType<NetworkNodeHostClientData>()
//                .SavePartialType()
//                .SaveProperty()

//                .GetProperty(x => x.Id)
//                .SetBinaryType<BinaryInt32>()
//                .SaveProperty()

//                .GetProperty(x => x.GameServer)
//                .SetPartialType<NetworkNodeHostClientData>()
//                .Back()
//                .SaveProperty();
//            ;
//        }

//        private static void BuildProfileInfoStructure(TypeStorage storage)
//        {
//            StructBuilder<ProfileInfo>
//                .GetStruct(storage)

//                .GetProperty(x => x.Id)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Username)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Age)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Country)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("ProfileInfo")
//#if !OnlyServer
//                .AppendScheme("RoomPlayerInfo")
//#endif
//                .SaveProperty()

//                .GetProperty(x => x.City)
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("ProfileInfo")
//#if !OnlyServer
//                .AppendScheme("RoomPlayerInfo")
//#endif
//                .SaveProperty()

//#if OnlyServer
//                .GetProperty("CountryPublic")
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty("CityPublic")
//                .SetBinaryType<BinaryString16>()
//                .SetSchemes("RoomPlayerInfo")
//                .SaveProperty()
//#endif
//                .GetProperty(x => x.Ounce)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "UpdateCoinsInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Denarius)
//                .SetBinaryType<BinaryInt32>()
//                .SetSchemes("ProfileInfo", "UpdateCoinsInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Premium)
//                .SetBinaryType<BinaryBool>()
//                .SetSchemes("ProfileInfo", "RoomPlayerInfo")
//                .SaveProperty()

//                .GetProperty(x => x.PremiumEndTime)
//                .SetBinaryType<BinaryNullable<BinaryDateTime, DateTime>>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.Admin)
//                .SetBinaryType<BinaryBool>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .GetProperty(x => x.CharacterCells)
//                .SetBinaryType<BinaryByte>()
//                .SetSchemes("ProfileInfo")
//                .SaveProperty()

//                .Compile();
//        }
    }
}
