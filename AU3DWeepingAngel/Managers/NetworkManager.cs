using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSG.Airlock;
using UnityEngine;
using AU3DWeepingAngel.Config;

namespace AU3DWeepingAngel.Managers
{
    internal static class NetworkManager
    {
        internal const byte Header = 87;
        internal const int Size = 18;
        internal static float LastUpdate;
        private static float NextNetworkSend;

        internal static void Send()
        {
            if (Time.time < NextNetworkSend) return;
            NextNetworkSend = Time.time + 0.05f;

            byte[] data = new byte[Size];
            data[0] = Header;
            BitConverter.GetBytes(GhostManager.AngelPosition.x).CopyTo(data, 1);
            BitConverter.GetBytes(GhostManager.AngelPosition.y).CopyTo(data, 5);
            BitConverter.GetBytes(GhostManager.AngelPosition.z).CopyTo(data, 9);
            BitConverter.GetBytes(GhostManager.GhostYaw).CopyTo(data, 13);
            data[17] = (byte)GhostManager.Countdown;

            Il2CppStructArray<byte> packet = data;
            foreach (PlayerState player in GameReferences.Spawn.ActivePlayerStates)
                if (player.PlayerId != GameReferences.Rig.PState.PlayerId)
                    GameReferences.Runner.SendReliableDataToPlayer(player.PlayerId, packet);
        }

        internal static void Read(byte[] data)
        {
            GhostManager.AngelPosition = new Vector3(BitConverter.ToSingle(data, 1), BitConverter.ToSingle(data, 5), BitConverter.ToSingle(data, 9));
            GhostManager.GhostYaw = BitConverter.ToSingle(data, 13);
            GhostManager.Countdown = data[17];
            LastUpdate = Time.time;
        }
    }
}
