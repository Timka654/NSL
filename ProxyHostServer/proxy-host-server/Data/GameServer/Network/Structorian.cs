using BinarySerializer;
using BinarySerializer.Builder;
using BinarySerializer.DefaultTypes;
using phs.Data.NodeHostServer.Info;
using phs.Data.GameServer.Info;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Helper.Configuration.Info;

namespace phs.Data.GameServer.Network
{
    public class Structorian
    {
        public static void BuildStructures(TypeStorage storage)
        {
            //BuildLobbyRoomInfoStructure(storage);

            //BuildCharacterStatisticsInfoStructure(storage);
            //BuildInventoryItemInfoStructure(storage);
            //BuildItemEnhanceInfoStructure(storage);
            //BuildLobbyPlayerInfoStructure(storage);

            //BuildConfigurationInfoStructure(storage);
            //BuildBattleModeInfoStructure(storage);
            //BuildMapInfoStructure(storage);
        }

        //private static void BuildLobbyRoomInfoStructure(TypeStorage storage)
        //{
        //    StructBuilder<LobbyRoomInfo>
        //        .GetStruct(storage)
        //        .SetSchemes("RoomInfoResult")

        //        .GetProperty(x => x.MapId)
        //        .SetBinaryType<BinaryInt16>()
        //        .SaveProperty()

        //        .GetProperty(x => x.Mode)
        //        .SetBinaryType<BinaryByte>()
        //        .SaveProperty()

        //        .GetProperty(x => x.EndTime)
        //        .SetBinaryType<BinaryNullable<BinaryDateTime, DateTime>>()
        //        .SaveProperty()

        //        .Compile();
        //}

        //private static void BuildItemEnhanceInfoStructure(TypeStorage storage)
        //{
        //    StructBuilder<ItemEnhanceInfo>
        //        .GetStruct(storage)
        //        .SetSchemes("PlayerConnectedResult")

        //        .GetProperty(x => x.HackDamageMin)
        //        .SetBinaryType<BinaryInt16>()
        //        .SaveProperty()

        //        .GetProperty(x => x.HackDamageMax)
        //        .SetBinaryType<BinaryInt16>()
        //        .SaveProperty()

        //        .GetProperty(x => x.ChopDamageMin)
        //        .SetBinaryType<BinaryInt16>()
        //        .SaveProperty()

        //        .GetProperty(x => x.ChopDamageMax)
        //        .SetBinaryType<BinaryInt16>()
        //        .SaveProperty()

        //        .GetProperty(x => x.Defense)
        //        .SetBinaryType<BinaryInt16>()
        //        .SaveProperty()

        //        .GetProperty(x => x.AttackSpeed)
        //        .SetBinaryType<BinaryInt16>()
        //        .SaveProperty()

        //        .GetProperty(x => x.RunningSpeed)
        //        .SetBinaryType<BinaryInt16>()
        //        .SaveProperty()

        //        .Compile();
        //}

        //private static void BuildInventoryItemInfoStructure(TypeStorage storage)
        //{
        //    StructBuilder<InventoryItemInfo>
        //        .GetStruct(storage)
        //        .SetSchemes("PlayerConnectedResult")

        //        .GetProperty(x => x.Id)
        //        .SetBinaryType<BinaryInt32>()
        //        .AppendScheme("PlayerDisconnectedResult")
        //        .SaveProperty()

        //        .GetProperty(x => x.ItemId)
        //        .SetBinaryType<BinaryInt32>()
        //        .SaveProperty()

        //        .GetProperty(x => x.WearType)
        //        .SetBinaryType<BinaryByte>()
        //        .SaveProperty()

        //        .GetProperty(x => x.Durability)
        //        .SetBinaryType<BinaryFloat32>()
        //        .AppendScheme("PlayerDisconnectedResult")
        //        .SaveProperty()

        //        .GetProperty(x => x.CurrentLevelEnhanceInfo)
        //        .SetPartialType<ItemEnhanceInfo>()
        //        .Back()
        //        .SaveProperty()


        //        .Compile();
        //}

        //private static void BuildCharacterStatisticsInfoStructure(TypeStorage storage)
        //{
        //    StructBuilder<CharacterStatisticsInfo>
        //        .GetStruct(storage)
        //        .SetSchemes("PlayerConnectedResult")

        //        .GetProperty(x => x.Mode)
        //        .SetBinaryType<BinaryByte>()
        //        .SaveProperty()

        //        .GetProperty(x => x.TotalCount)
        //        .SetBinaryType<BinaryInt32>()
        //        .SaveProperty()

        //        .GetProperty(x => x.WinsCount)
        //        .SetBinaryType<BinaryInt32>()
        //        .SaveProperty()

        //        .GetProperty(x => x.WinsCount)
        //        .SetBinaryType<BinaryInt32>()
        //        .SaveProperty()

        //        .GetProperty(x => x.DefeatsCount)
        //        .SetBinaryType<BinaryInt32>()
        //        .SaveProperty()

        //        .Compile();
        //}

        //private static void BuildLobbyPlayerInfoStructure(TypeStorage storage)
        //{
        //    StructBuilder<LobbyPlayerInfo>
        //        .GetStruct(storage)
        //        .SetSchemes("PlayerConnectedResult")

        //        .GetProperty(x => x.RoomId)
        //        .SetBinaryType<BinaryInt32>()
        //        .SetSchemes("PlayerDisconnected")
        //        .SaveProperty()

        //        .GetProperty(x => x.User)
        //        .AppendScheme("PlayerDisconnectedResult")
        //        .SetPartialType<ProfileInfo>()
        //            .SetSchemes("PlayerConnectedResult")
        //                .GetProperty(x => x.Id)
        //                .SetBinaryType<BinaryInt32>()
        //                .AppendScheme("PlayerDisconnectedResult")
        //                .SaveProperty()
        //                .GetProperty(x => x.Username)
        //                .SetBinaryType<BinaryString16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.Premium)
        //                .SetBinaryType<BinaryBool>()
        //                .SaveProperty()
        //        .SavePartialType()
        //        .SaveProperty()

        //        .GetProperty(x => x.RewardInfo)
        //        .SetSchemes("PlayerDisconnectedResult")
        //        .SetPartialType<PlayerRewardInfo>()
        //                .SetSchemes("PlayerDisconnectedResult")
        //                .GetProperty(x => x.RewardExp)
        //                .SetBinaryType<BinaryInt32>()
        //                .SaveProperty()
        //                .GetProperty(x => x.RewardGold)
        //                .SetBinaryType<BinaryInt32>()
        //                .SaveProperty()
        //                .GetProperty(x => x.RewardSilver)
        //                .SetBinaryType<BinaryInt32>()
        //                .SaveProperty()
        //                .GetProperty(x => x.Win)
        //                .SetBinaryType<BinaryBool>()
        //                .SaveProperty()
        //        .SavePartialType()
        //        .SaveProperty()

        //        .GetProperty(x => x.CurrentCharacter)
        //        .AppendScheme("PlayerDisconnectedResult")
        //        .SetPartialType<CharacterInfo>()
        //            .SetSchemes("PlayerConnectedResult")
        //                .GetProperty(x => x.Id)
        //                .SetBinaryType<BinaryInt32>()
        //                .AppendScheme("PlayerDisconnectedResult")
        //                .SaveProperty()
        //                .GetProperty(x => x.CharacterTypeId)
        //                .SetBinaryType<BinaryInt32>()
        //                .SaveProperty()
        //                .GetProperty(x => x.Name)
        //                .SetBinaryType<BinaryString16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.Level)
        //                .SetBinaryType<BinaryByte>()
        //                .SaveProperty()
        //                .GetProperty(x => x.Hp)
        //                .SetBinaryType<BinaryInt16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.HackDamageMin)
        //                .SetBinaryType<BinaryInt16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.HackDamageMax)
        //                .SetBinaryType<BinaryInt16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.ChopDamageMin)
        //                .SetBinaryType<BinaryInt16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.ChopDamageMax)
        //                .SetBinaryType<BinaryInt16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.Defense)
        //                .SetBinaryType<BinaryInt16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.AttackSpeed)
        //                .SetBinaryType<BinaryInt16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.RunningSpeed)
        //                .SetBinaryType<BinaryInt16>()
        //                .SaveProperty()
        //                .GetProperty(x => x.StatisticsMap)
        //                .SetBinaryType<BinaryDictionary16<BinaryByte, CharacterStatisticsInfo>>()
        //                .SaveProperty()
        //                .GetProperty(x => x.EquipedItemList)
        //                .AppendScheme("PlayerDisconnectedResult")
        //                .SetBinaryType<BinaryList16<InventoryItemInfo>>()
        //                .SaveProperty()
        //        .SavePartialType()
        //        .SaveProperty()

        //        .GetProperty(x => x.TeamId)
        //            .SetBinaryType<BinaryByte>()
        //        .SaveProperty()

        //        .GetProperty(x => x.GameConnectionId)
        //            .SetSchemes("PlayerDisconnected")
        //            .SetBinaryType<BinaryInt32>()
        //        .SaveProperty()
        //        .Compile();
        //}

        //private static void BuildConfigurationInfoStructure(TypeStorage storage)
        //{
        //    StructBuilder<ConfigurationInfo>
        //        .GetStruct(storage)

        //        .GetProperty(x => x.Name)
        //        .SetBinaryType<BinaryString16>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()

        //        .GetProperty(x => x.Value)
        //        .SetBinaryType<BinaryString16>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .Compile();
        //}

        //private static void BuildBattleModeInfoStructure(TypeStorage storage)
        //{
        //    StructBuilder<BattleModeInfo>.GetStruct(storage)
        //        .GetProperty(x => x.Type)
        //        .SetBinaryType<BinaryByte>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.KillGoldRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.KillExpRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.DeathGoldRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.DeathExpRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.WinGoldRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.WinExpRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.DefeatGoldRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.DefeatExpRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.ScoreRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.DeathSilverRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.WinSilverRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.DefeatSilverRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.KillSilverRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .GetProperty(x => x.ScoreSilverRate)
        //        .SetBinaryType<BinaryFloat64>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()
        //        .Compile();
        //}

        //private static void BuildMapInfoStructure(TypeStorage storage)
        //{
        //    StructBuilder<MapInfo>
        //        .GetStruct(storage)

        //        .GetProperty(x => x.Id)
        //        .SetBinaryType<BinaryInt16>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()

        //        .GetProperty(x => x.MeshPath)
        //        .SetBinaryType<BinaryString16>()
        //        .SetSchemes("ServerInfoData")
        //        .SaveProperty()

        //        .Compile();
        //}
    }
}
