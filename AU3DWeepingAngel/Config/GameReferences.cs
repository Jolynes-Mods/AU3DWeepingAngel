using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Sabotage;
using Il2CppSG.Airlock.XR;

namespace AU3DWeepingAngel.Config
{
    internal class GameReferences
    {
        public static SpawnManager Spawn;
        public static XRRig Rig;
        public static NetworkRunner Runner;
        public static GameStateManager GameState;
        public static GameSession Session;
        public static NetworkedKillBehaviour Killing;
        public static AirlockPeer Peer;
        public static SabotageManager Sabotage;

        internal static bool Allowed => Settings.WeepingAngelEnabled && Session != null && (Session.IsPrivate || Settings.ModdedLobby);

        internal static void ResetReferences()
        {
            Spawn = UnityEngine.Object.FindObjectOfType<SpawnManager>();
            Rig = UnityEngine.Object.FindObjectOfType<XRRig>();
            Runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
            GameState = UnityEngine.Object.FindObjectOfType<GameStateManager>();
            Session = UnityEngine.Object.FindObjectOfType<GameSession>();
            Killing = UnityEngine.Object.FindObjectOfType<NetworkedKillBehaviour>();
            Peer = UnityEngine.Object.FindObjectOfType<AirlockPeer>();
            Sabotage = UnityEngine.Object.FindObjectOfType<SabotageManager>();
        }
    }
}
