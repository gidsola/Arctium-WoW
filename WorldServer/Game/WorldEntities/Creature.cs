/*
 * Copyright (C) 2012-2014 Arctium <http://arctium.org>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Framework.Database;
using WorldServer.Game.ObjectDefines;

namespace WorldServer.Game.WorldEntities
{
    public class Creature
    {
        public CreatureStats Stats;
        public CreatureData Data;
        public CreatureDataAddon DataAddon;

        public Creature() { }
        public Creature(int id)
        {
            SQLResult result = DB.World.Select("SELECT * FROM `creature_stats` WHERE `Id` = ?", id);

            if (result.Count != 0)
            {
                Stats = new CreatureStats();

                Stats.Id       = result.Read<int>(0, "Id");
                Stats.Name     = result.Read<string>(0, "Name");
                Stats.SubName  = result.Read<string>(0, "SubName");
                Stats.IconName = result.Read<string>(0, "IconName");

                for (int i = 0; i < Stats.Flag.Capacity; i++)
                    Stats.Flag.Add(result.Read<int>(0, "Flag", i));

                Stats.Type   = result.Read<int>(0, "Type");
                Stats.Family = result.Read<int>(0, "Family");
                Stats.Rank   = result.Read<int>(0, "Rank");

                for (int i = 0; i < Stats.QuestKillNpcId.Capacity; i++)
                    Stats.QuestKillNpcId.Add(result.Read<int>(0, "QuestKillNpcId", i));

                for (int i = 0; i < Stats.DisplayInfoId.Capacity; i++)
                    Stats.DisplayInfoId.Add(result.Read<int>(0, "DisplayInfoId", i));

                Stats.HealthModifier = result.Read<float>(0, "HealthModifier");
                Stats.PowerModifier  = result.Read<float>(0, "PowerModifier");
                Stats.RacialLeader   = result.Read<byte>(0, "RacialLeader");

                for (int i = 0; i < Stats.QuestItemId.Capacity; i++)
                {
                    var questItem = result.Read<int>(0, "QuestItemId", i);

                    if (questItem != 0)
                        Stats.QuestItemId.Add(questItem);
                }

                Stats.MovementInfoId    = result.Read<int>(0, "MovementInfoId");
                Stats.ExpansionRequired = result.Read<int>(0, "ExpansionRequired");
            }

            result = DB.World.Select("SELECT * FROM `creature_data` WHERE `Id` = ?", id);

            if (result.Count != 0)
            {
                Data = new CreatureData();

                Data.Level      = result.Read<byte>(0, "Level");
                Data.Class      = result.Read<byte>(0, "Class");
                Data.BaseHealth = result.Read<int>(0, "BaseHealth");
                Data.MaxHealth  = result.Read<int>(0, "MaxHealth");
                Data.BaseMana   = result.Read<int>(0, "BaseMana");
                Data.Faction    = result.Read<int>(0, "Faction");
                Data.Scale      = result.Read<int>(0, "Scale");
                Data.MinDmg     = result.Read<int>(0, "MinDmg");
                Data.MaxDmg     = result.Read<int>(0, "MaxDmg");
                Data.UnitFlags  = result.Read<uint>(0, "UnitFlags");
                Data.UnitFlags2 = result.Read<uint>(0, "UnitFlags2");
                Data.NpcFlags   = result.Read<int>(0, "NpcFlags");
            }

            result = DB.World.Select("SELECT * FROM `creature_data_addon` WHERE `Id` = ?", id);

            if (result.Count != 0)
            {
                DataAddon = new CreatureDataAddon();

                DataAddon.PathId = result.Read<int>(0, "PathId");
                DataAddon.MountId = result.Read<int>(0, "MountId");
                DataAddon.Bytes1 = result.Read<int>(0, "Bytes1");
                DataAddon.Bytes2 = result.Read<int>(0, "Bytes2");
                DataAddon.EmoteState = result.Read<int>(0, "EmoteState");
                DataAddon.AuraState = result.Read<string>(0, "AuraState");
            }
        }
    }
}
