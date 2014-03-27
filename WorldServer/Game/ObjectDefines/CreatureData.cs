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

namespace WorldServer.Game.ObjectDefines
{
    public class CreatureData
    {
        public byte Level;
        public byte Class;
        public int BaseHealth;
        public int MaxHealth;
        public int BaseMana;
        public int Faction;
        public float Scale;
        public int MinDmg;
        public int MaxDmg;
        public uint UnitFlags;
        public uint UnitFlags2;
        public int NpcFlags;
    }

    public class CreatureDataAddon
    {
        public int PathId;
        public int MountId;
        public int Bytes1;
        public int Bytes2;
        public int EmoteState;
        public string AuraState;
    }

    public class CreatureSpawnAddon
    {
        public int PathId;
        public int MountId;
        public int Bytes1;
        public int Bytes2;
        public float HoverHeight;
        public int EmoteState;
        public string AuraState;
    }
}
