using HarmonyLib;
using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Cutscenes;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using AU3DWeepingAngel.Config;
using AU3DWeepingAngel.Managers;

namespace AU3DWeepingAngel.Patches
{
    internal class GamePatches
    {
        [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.Fusion_Simulation_ICallbacks_OnReliableData))]
        public class ReceivePatch
        {
            public static bool Prefix(Il2CppStructArray<byte> dataArray)
            {
                if (dataArray == null || dataArray.Length != NetworkManager.Size || dataArray[0] != NetworkManager.Header) return true;
                if (!Settings.IsHost) NetworkManager.Read(dataArray);
                return false;
            }
        }

        [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoles))]
        public class NoImpostorsPatch
        {
            private static int impostors;

            public static void Prefix(RoleManager __instance)
            {
                if (!Settings.IsHost || !GameReferences.Allowed) return;
                impostors = __instance.NumImpostersVar.Value;
                __instance.NumImpostersVar.Value = 0;
            }

            public static void Postfix(RoleManager __instance)
            {
                if (Settings.IsHost && GameReferences.Allowed) __instance.NumImpostersVar.Value = impostors;
            }
        }

        [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.CheckLivingPlayers))]
        public class WinConditionPatch
        {
            public static bool Prefix(GameStateManager __instance, ref bool __result)
            {
                if (!Settings.IsHost || !GameReferences.Allowed) return true;

                __result = false;
                if (!__instance.InTaskState() && !__instance.InVotingState()) return false;

                foreach (PlayerState player in GameReferences.Spawn.ActivePlayerStates)
                    if (player.IsAlive && !player.IsSpectating) return false;

                MelonLogger.Msg("Weeping Ghost wins!");
                __instance.GameEndReasonIndex = (int)EndGameReasonsData.EndGameReason.NotEnoughCrewmates;
                __instance.ReportWin(GameTeam.Impostor);
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(CutsceneSetupEndGame), nameof(CutsceneSetupEndGame.DetermineEndScreenText))]
        public class EndTextPatch
        {
            public static void Postfix(CutsceneSetupEndGame __instance, int endReason)
            {
                if (GameReferences.Allowed && endReason == (int)EndGameReasonsData.EndGameReason.NotEnoughCrewmates)
                    __instance._endReasonText.text = "The Weeping Angel killed everyone!";
            }
        }

        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new[] { typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
        public class CallVotePatch
        {
            public static bool Prefix()
            {
                return !GameReferences.Allowed;
            }
        }

        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new[] { typeof(int), typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
        public class ReportVotePatch
        {
            public static bool Prefix()
            {
                return !GameReferences.Allowed;
            }
        }

        [HarmonyPatch(typeof(NetworkedKillBehaviour), nameof(NetworkedKillBehaviour.RPC_TargetedAction))]
        public class TargetedActionPatch
        {
            public static bool Prefix(PlayerRef targetedPlayer, PlayerRef perpetrator, int action)
            {
                if (!Settings.IsHost || !GameReferences.Allowed) return true;

                if (action == (int)ProximityTargetedAction.Kill) return false;

                return true;
            }
        }
    }
}
