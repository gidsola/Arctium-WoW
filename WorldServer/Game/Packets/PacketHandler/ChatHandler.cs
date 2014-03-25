﻿/*
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

using Framework.Constants;
using Framework.Constants.NetMessage;
using Framework.Network.Packets;
using Framework.ObjectDefines;
using WorldServer.Game.Chat;
using WorldServer.Network;

namespace WorldServer.Game.Packets.PacketHandler
{
    public class ChatHandler : Globals
    {
        [Opcode(ClientMessage.ChatMessageSay, "18019")]
        public static void HandleChatMessageSay(ref PacketReader packet, WorldClass session)
        {
            BitUnpack BitUnpack = new BitUnpack(packet);

            var language = packet.Read<int>();

            var messageLength = BitUnpack.GetBits<byte>(8);
            var message = packet.ReadString(messageLength);

            ChatMessageValues chatMessage = new ChatMessageValues(MessageType.ChatMessageSay, message, true, true, session.Character.Name);
            chatMessage.Language = (byte)language;

            if (ChatCommandParser.CheckForCommand(message))
                ChatCommandParser.ExecuteChatHandler(message, session);
            else
                SendMessage(ref session, chatMessage);
        }

        [Opcode(ClientMessage.ChatMessageYell, "18019")]
        public static void HandleChatMessageYell(ref PacketReader packet, WorldClass session)
        {
            BitUnpack BitUnpack = new BitUnpack(packet);

            var language = packet.Read<int>();

            var messageLength = packet.ReadByte();
            var message = packet.ReadString(messageLength);

            ChatMessageValues chatMessage = new ChatMessageValues(MessageType.ChatMessageYell, message, true, true, session.Character.Name);
            chatMessage.Language = (byte)language;

            SendMessage(ref session, chatMessage);
        }

        [Opcode(ClientMessage.ChatMessageWhisper, "18019")]
        public static void HandleChatMessageWhisper(ref PacketReader packet, WorldClass session)
        {
            BitUnpack BitUnpack = new BitUnpack(packet);

            var language = packet.Read<int>();

            var messageLength = BitUnpack.GetBits<byte>(8);
            var nameLength = BitUnpack.GetBits<byte>(9);

            string receiverName = packet.ReadString(nameLength);
            string message = packet.ReadString(messageLength);

            WorldClass rSession = WorldMgr.GetSession(receiverName);

            if (rSession == null)
                return;

            ChatMessageValues chatMessage = new ChatMessageValues(MessageType.ChatMessageWhisperInform, message, false, true, session.Character.Name);
            SendMessage(ref session, chatMessage, rSession);

            chatMessage = new ChatMessageValues(MessageType.ChatMessageWhisper, message, false, true, session.Character.Name);
            SendMessage(ref rSession, chatMessage, session);
        }

        public static void SendMessage(ref WorldClass session, ChatMessageValues chatMessage, WorldClass pSession = null)
        {
            byte[] GuidMask = { 2, 7, 0, 3, 4, 6, 1, 5 };
            byte[] GuidMask3 = { 5, 7, 6, 4, 3, 2, 1, 0 };
            byte[] GuidBytes = { 6, 1, 0, 2, 4, 5, 7, 3 };
            byte[] GuidBytes3 = { 4, 2, 3, 0, 6, 7, 5, 1 };

            var pChar = session.Character;
            var guid = pChar.Guid;

            if (pSession != null)
                guid = pSession.Character.Guid;

            PacketWriter chat = new PacketWriter(ServerMessage.Chat);
            BitPack BitPack = new BitPack(chat, guid);

            BitPack.Write(1);
            BitPack.Write(0);

            BitPack.Write(0, 8);

            BitPack.Write(1);

            BitPack.Write(!chatMessage.HasLanguage);

            BitPack.WriteGuidMask(GuidMask);

            BitPack.Write(0);

            BitPack.Write(1);
            BitPack.Write(1);

            BitPack.Write(chatMessage.From.Length == 0);
            BitPack.Write(chatMessage.Message.Length == 0);

            BitPack.Write(0);

            BitPack.WriteGuidMask(GuidMask3);

            BitPack.Write(!chatMessage.HasRealmId);

            if (chatMessage.From.Length > 0)
                BitPack.WriteStringLength(chatMessage.From, 11);

            BitPack.Write(1);

            BitPack.Write(0, 8);

            BitPack.Write(1);

            if (chatMessage.Message.Length > 0)
                BitPack.WriteStringLength(chatMessage.Message, 12);

            BitPack.Write(0);

            BitPack.Write(1);
            BitPack.Write(1);

            BitPack.Write(!chatMessage.HasRealmId);

            BitPack.Flush();

            chat.WriteUInt8((byte)chatMessage.ChatType);

            if (chatMessage.HasRealmId)
                chat.WriteUInt32(chatMessage.RealmId);

            BitPack.WriteGuidBytes(GuidBytes3);
            BitPack.WriteGuidBytes(GuidBytes);

            chat.WriteString(chatMessage.Message, false);
            chat.WriteString(chatMessage.From, false);

            if (chatMessage.HasLanguage)
                chat.WriteUInt8(chatMessage.Language);

            if (chatMessage.HasRealmId) 
                chat.WriteUInt32(chatMessage.RealmId);

            switch (chatMessage.ChatType)
            {
                case MessageType.ChatMessageSay:
                    WorldMgr.SendByDist(pChar, chat, 625);
                    break;
                case MessageType.ChatMessageYell:
                    WorldMgr.SendByDist(pChar, chat, 90000);
                    break;
                default:
                    session.Send(ref chat);
                    break;
            }
        }
    }
}
