﻿/*
 * Copyright (C) 2012-2013 Arctium <http://arctium.org>
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

using Framework.Constants;
using Framework.Constants.NetMessage;
using Framework.Constants.ObjectSettings;
using Framework.Network.Packets;
using Framework.ObjectDefines;
using Framework.Singleton;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WorldServer.Game.Spawns;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;

namespace WorldServer.Game.Managers
{
    public sealed class WorldManager : SingletonBase<WorldManager>
    {
        public ConcurrentDictionary<ulong, WorldClass> Sessions;
        public WorldClass Session { get; set; }

        static readonly object taskObject = new object();

        WorldManager()
        {
            Sessions = new ConcurrentDictionary<ulong, WorldClass>();

            StartRangeUpdateTimers();
        }

        public void StartRangeUpdateTimers()
        {
            var updateTask = new Thread(UpdateTask);
            updateTask.IsBackground = true;
            updateTask.Start();
        }

        void UpdateTask()
        {
            while (true)
            {
                lock (taskObject)
                {
                    Thread.Sleep(50);

                    foreach (var s in Sessions.ToList())
                    {
                        var session = s.Value;
                        var pChar = session.Character;

                        WriteInRangeObjects(Globals.SpawnMgr.GetInRangeCreatures(pChar), session, ObjectType.Unit);
                        WriteInRangeObjects(Globals.SpawnMgr.GetInRangeGameObjects(pChar), session, ObjectType.GameObject);
                        WriteInRangeObjects(GetInRangeCharacter(pChar), session, ObjectType.Player);

                        WriteOutOfRangeObjects(Globals.SpawnMgr.GetOutOfRangeCreatures(pChar), session);
                        WriteOutOfRangeObjects(Globals.SpawnMgr.GetOutOfRangeGameObjects(pChar), session);
                        WriteOutOfRangeObjects(GetOutOfRangeCharacter(pChar), session);
                    }
                }
            }
        }

        public bool AddSession(ulong guid, ref WorldClass session)
        {
            return Sessions.TryAdd(guid, session);
        }

        public WorldClass DeleteSession(ulong guid)
        {
            WorldClass removedSession;
            Sessions.TryRemove(guid, out removedSession);

            return removedSession;
        }

        public WorldClass GetSession(string name)
        {
            foreach (var s in Sessions)
                if (s.Value.Character.Name == name)
                    return s.Value;

            return null;
        }

        public WorldClass GetSession(ulong guid)
        {
            WorldClass session;
            Sessions.TryGetValue(guid, out session);

            return session;
        }

        public void WriteCreateObject(ref PacketWriter updateObject, WorldObject obj, UpdateFlag updateFlags, ObjectType type)
        {
            updateObject.WriteUInt8((byte)UpdateType.CreateObject);
            updateObject.WriteGuid(obj.Guid);
            updateObject.WriteUInt8((byte)type);

            Globals.WorldMgr.WriteUpdateObjectMovement(ref updateObject, ref obj, updateFlags);

            obj.SetUpdateFields();
            obj.WriteUpdateFields(ref updateObject);
            obj.WriteDynamicUpdateFields(ref updateObject);
        }

        void WriteInRangeObjects(IEnumerable<WorldObject> objects, WorldClass session, ObjectType type)
        {
            var pChar = session.Character;
            var count = objects.Count();
            var updateFlags = UpdateFlag.Rotation;

            if (count > 0)
            {
                updateFlags |= type == ObjectType.GameObject ? UpdateFlag.StationaryPosition : UpdateFlag.Alive;

                PacketWriter updateObject = new PacketWriter(ServerMessage.ObjectUpdate);
                updateObject.WriteUInt16((ushort)pChar.Map);
                updateObject.WriteUInt32((uint)count);

                foreach (var o in objects)
                {
                    WorldObject obj = o;

                    if (!pChar.InRangeObjects.ContainsKey(o.Guid))
                    {
                        WriteCreateObject(ref updateObject, obj, updateFlags, type);

                        if (pChar.Guid != o.Guid)
                            pChar.InRangeObjects.Add(obj.Guid, obj);
                    }
                }

                session.Send(ref updateObject);
            }
        }

        void WriteOutOfRangeObjects(IEnumerable<WorldObject> objects, WorldClass session)
        {
            var pChar = session.Character;
            var count = objects.Count();

            if (count > 0)
            {
                PacketWriter updateObject = new PacketWriter(ServerMessage.ObjectUpdate);

                updateObject.WriteUInt16((ushort)pChar.Map);
                updateObject.WriteUInt32(1);
                updateObject.WriteUInt8((byte)UpdateType.OutOfRange);
                updateObject.WriteUInt32((uint)count);

                foreach (var o in objects)
                {
                    updateObject.WriteGuid(o.Guid);

                    pChar.InRangeObjects.Remove(o.Guid);
                }

                session.Send(ref updateObject);
            }
        }

        public IEnumerable<Character> GetInRangeCharacter(WorldObject obj)
        {
            var tempSessions = new Dictionary<ulong, WorldClass>(Sessions);
            tempSessions.Remove(obj.Guid);

            foreach (var c in tempSessions.ToList())
                if (!obj.ToCharacter().InRangeObjects.ContainsKey(c.Key))
                    if (obj.CheckUpdateDistance(c.Value.Character))
                        yield return c.Value.Character;
        }

        public IEnumerable<Character> GetOutOfRangeCharacter(WorldObject obj)
        {
            var tempSessions = new Dictionary<ulong, WorldClass>(Sessions);
            tempSessions.Remove(obj.Guid);

            foreach (var c in tempSessions.ToList())
                if (obj.ToCharacter().InRangeObjects.ContainsKey(c.Key))
                    if (!obj.CheckUpdateDistance(c.Value.Character))
                        yield return c.Value.Character;
        }

        public void WriteAccountDataTimes(AccountDataMasks mask, ref WorldClass session)
        {
            PacketWriter accountDataTimes = new PacketWriter(ServerMessage.AccountDataTimes);

            for (int i = 0; i < 8; i++)
                accountDataTimes.WriteUInt32(0);
  
            accountDataTimes.WriteUInt32((uint)mask);
            accountDataTimes.WriteUnixTime();
            accountDataTimes.WriteUInt8(0);

            session.Send(ref accountDataTimes);
        }

        public void SendToInRangeCharacter(Character pChar, PacketWriter packet)
        {
            foreach (var c in Sessions.ToList())
            {
                WorldObject iChar;
                if (pChar.InRangeObjects.TryGetValue(c.Value.Character.Guid, out iChar))
                    c.Value.Send(ref packet);
            }
        }

        public void WriteUpdateObjectMovement(ref PacketWriter packet, ref WorldObject wObject, UpdateFlag updateFlags)
        {
            ObjectMovementValues values = new ObjectMovementValues(updateFlags);
            BitPack BitPack = new BitPack(packet, wObject.Guid);

            BitPack.Write(values.HasTarget);
            BitPack.Write(values.Bit1);
            BitPack.Write(values.HasUnknown2);
            BitPack.Write(0);                       // New in 5.1.0, 784, Unknown
            BitPack.Write(values.Bit0);
            BitPack.Write(values.Bit3);
            BitPack.Write(0);                       // New in 5.1.0, 654, Unknown
            BitPack.Write(values.IsSelf);
            BitPack.Write(values.HasGoTransportPosition);
            BitPack.Write(values.HasUnknown);
            BitPack.Write(wObject is GameObjectSpawn);
            BitPack.Write(values.IsAlive);
            BitPack.Write(values.HasUnknown4);
            BitPack.Write(values.HasAnimKits);
            BitPack.Write(values.HasUnknown3);
            BitPack.Write(values.IsVehicle);
            BitPack.Write(values.HasStationaryPosition);
            BitPack.Write(values.Bit2);
            BitPack.Write(values.BitCounter, 22);

            if (values.IsAlive)
            {
                BitPack.WriteGuidMask(7);
                BitPack.Write(0, 19);               // BitCounter_Alive_1
                BitPack.Write(0);                   // IsInterpolated, not implanted
                BitPack.Write(values.IsTransport);
                BitPack.Write(0);                   // Unknown_Alive_1
                BitPack.WriteGuidMask(0);
                BitPack.Write(1);                   // Pitch or splineElevation, not implanted
                BitPack.Write(true);                // Movementflags are not implanted
                BitPack.Write(0);                   // HasSplineData, don't write simple basic splineData
                BitPack.WriteGuidMask(1);
                BitPack.Write(true);                // MovementFlags2 are not implanted
                BitPack.Write(0, 22);               // BitCounter_Alive_2
                BitPack.WriteGuidMask(3, 5, 6);
                BitPack.Write(0);                   // Unknown_Alive_3
                BitPack.Write(!values.HasRotation);
                BitPack.Write(1);                   // Pitch or splineElevation, not implanted
                BitPack.Write(1);                   // Unknown_Alive_2, Reversed
                BitPack.WriteGuidMask(2);
                BitPack.Write(!values.IsAlive);
                BitPack.Write(0);                   // Unknown_Alive_4
                BitPack.WriteGuidMask(4);
            }

            BitPack.Flush();

            if (values.IsAlive)
            {
                BitPack.WriteGuidBytes(5);
                packet.WriteFloat((float)MovementSpeed.SwimSpeed);
                packet.WriteFloat((float)MovementSpeed.WalkSpeed);
                BitPack.WriteGuidBytes(6);
                packet.WriteFloat((float)MovementSpeed.RunSpeed);
                BitPack.WriteGuidBytes(0);
                packet.WriteFloat(wObject.Position.Z);
                packet.WriteFloat((float)MovementSpeed.FlySpeed);
                packet.WriteFloat(wObject.Position.Y);
                packet.WriteFloat(wObject.Position.X);
                BitPack.WriteGuidBytes(4, 3);
                packet.WriteFloat((float)MovementSpeed.TurnSpeed);
                BitPack.WriteGuidBytes(2);
                packet.WriteUInt32(0);
                packet.WriteFloat(wObject.Position.O);
                packet.WriteFloat((float)MovementSpeed.FlyBackSpeed);
                packet.WriteFloat((float)MovementSpeed.SwimBackSpeed);
                packet.WriteFloat((float)MovementSpeed.RunBackSpeed);
                packet.WriteFloat((float)MovementSpeed.PitchSpeed);
                BitPack.WriteGuidBytes(1, 7);
            }

            if (values.HasStationaryPosition)
            {
                packet.WriteFloat(wObject.Position.Z);
                packet.WriteFloat(wObject.Position.O);
                packet.WriteFloat(wObject.Position.Y);
                packet.WriteFloat(wObject.Position.X);
            }

            if (wObject is GameObjectSpawn)
                packet.WriteInt64(Quaternion.GetCompressed(wObject.Position.O));
        }
    }
}
