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

namespace Framework.Constants.NetMessage
{
    public enum ClientMessage : ushort
    {
        #region ChatMessages
        ChatMessageSay              = 0x0C41,
        ChatMessageYell             = 0x0C43,
        ChatMessageWhisper          = 0x0D60,
        #endregion

        #region UserRouterClient
        AuthSession                 = 0x1A51,
        Ping                        = 0x1070,
        LogDisconnect               = 0x1A13,
        #endregion

        #region Legacy
        ActivePlayer                = 0x173A,
        #endregion

        #region JAM
        ObjectUpdateFailed          = 0x0882,
        ViolenceLevel               = 0x05A0,
        DBQueryBulk                 = 0x16C2,
        GenerateRandomCharacterName = 0x1DB9,
        EnumCharacters              = 0x12C2,
        PlayerLogin                 = 0x17D3,
        CharacterSelectScreen       = 0x1282,
        LoadingScreenNotify         = 0x1691,
        SetActionButton             = 0x1393,
        CreateCharacter             = 0x09B9,
        QueryPlayerName             = 0x0DB3,
        QueryRealmName              = 0x1899,
        ReadyForAccountDataTimes    = 0x13CB,
        UITimeRequest               = 0x1CA3,
        CharDelete                  = 0x113B,
        CliSetSpecialization        = 0x04AA,
        CliLearnTalents             = 0x1F5A,
        CliQueryCreature            = 0x1E72,
        CliQueryGameObject          = 0x14EA,
        CliQueryNPCText             = 0x12FA,
        CliTalkToGossip             = 0x05F6,
        CliLogoutRequest            = 0x0476,
        CliSetSelection             = 0x10D5,
        #endregion

        #region PlayerMove
        MoveStartForward            = 0x041B,
        MoveStartBackward           = 0x0459,
        MoveStop                    = 0x0570,
        MoveStartStrafeLeft         = 0x0873,
        MoveStartStrafeRight        = 0x0C12,
        MoveStopStrafe              = 0x0171,
        MoveJump                    = 0x0438,
        MoveStartTurnLeft           = 0x011B,
        MoveStartTurnRight          = 0x0411,
        MoveStopTurn                = 0x0530,
        MoveStartPitchUp            = 0x0079,
        MoveStartPitchDown          = 0x093B,
        MoveStopPitch               = 0x0071,
        MoveSetRunMode              = 0x0878,
        MoveSetWalkMode             = 0x0138,
        MoveFallLand                = 0x055B,
        MoveStartSwim               = 0x0871,
        MoveStopSwim                = 0x0578,
        MoveToggleCollisionCheat    = 0x0959,
        MoveSetFacing               = 0x005A,
        MoveSetPitch                = 0x047A,
        MoveHeartbeat               = 0x017B,
        MoveFallReset               = 0x0450,
        MoveSetFly                  = 0x0551,
        MoveStartAscend             = 0x0430,
        MoveStopAscend              = 0x0012,
        MoveChangeTransport         = 0x0052,
        MoveStartDescend            = 0x0132,
        MoveDismissVehicle          = 0x0979,
        #endregion

        TransferInitiate            = 0x4F57,
    }
}
