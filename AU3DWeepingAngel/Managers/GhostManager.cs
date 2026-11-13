using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Sabotage;
using Il2CppSG.Airlock.XR;
using MelonLoader;
using UnityEngine;
using AU3DWeepingAngel.Config;
using static Il2CppSG.Airlock.Sabotage.Sabotage;

namespace AU3DWeepingAngel.Managers
{
    internal static class GhostManager
    {
        internal static Vector3 AngelPosition;
        internal static float GhostYaw;
        internal static int Countdown;
        internal static bool Active;

        private static readonly Dictionary<int, List<Vector3>> trails = new Dictionary<int, List<Vector3>>();
        private static PlayerState target;
        private static Vector3 GoalPosition;
        private static float StartTime, NextThink, StareTime, LastSeen;
        private static int mask;

        internal static void Update()
        {
            if (!Active) Appear();

            if (Time.time >= NextThink)
            {
                NextThink = Time.time + 0.25f;
                Think();
            }

            if (Time.time - StartTime > 5f && target != null) Move();

            NetworkManager.Send();
        }

        private static void Appear()
        {
            Active = true;
            trails.Clear();
            target = null;
            StareTime = 0f;
            Countdown = 0;
            StartTime = Time.time;
            mask = 1 << LayerMask.NameToLayer("Obstacle");

            Vector3 center = GameReferences.Rig.PState.LocomotionPlayer.RigidbodyPosition;
            AngelPosition = center;
            GoalPosition = center;
            for (int angle = 0; angle < 360; angle += 45)
            {
                Vector3 spot = center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 5f;
                if (!CanWalk(center, spot)) continue;
                AngelPosition = spot;
                GoalPosition = spot;
                break;
            }

            MelonLogger.Msg("Weeping Ghost appeared");
        }

        private static void Think()
        {
            target = null;
            foreach (PlayerState player in GameReferences.Spawn.ActivePlayerStates)
            {
                if (!player.IsAlive || player.IsSpectating) continue;

                if (!trails.ContainsKey(player.PlayerId)) trails[player.PlayerId] = new List<Vector3>();
                List<Vector3> trail = trails[player.PlayerId];
                if (trail.Count == 0 || Vector3.Distance(trail[trail.Count - 1], player.LocomotionPlayer.RigidbodyPosition) > 0.75f) trail.Add(player.LocomotionPlayer.RigidbodyPosition);
                if (trail.Count > 300) trail.RemoveAt(0);

                if (target == null || Vector3.Distance(AngelPosition, player.LocomotionPlayer.RigidbodyPosition) < Vector3.Distance(AngelPosition, target.LocomotionPlayer.RigidbodyPosition))
                    target = player;
            }

            CheckLights();
            if (target == null) return;

            GoalPosition = target.LocomotionPlayer.RigidbodyPosition;
            if (CanWalk(AngelPosition, GoalPosition) || Follow(trails[target.PlayerId])) return;
            foreach (List<Vector3> trail in trails.Values)
                if (Follow(trail)) return;
            GoalPosition = AngelPosition;
        }

        private static bool Follow(List<Vector3> trail)
        {
            for (int step = trail.Count - 1; step >= 0; step--)
            {
                if (!CanWalk(AngelPosition, trail[step])) continue;
                GoalPosition = trail[step];
                return true;
            }
            return false;
        }

        private static void Move()
        {
            bool watched = Watched();
            if (watched)
            {
                StareTime += Time.deltaTime;
                LastSeen = Time.time;
            }
            else if (Time.time - LastSeen > 1f) StareTime = 0f;

            Countdown = StareTime >= 45f && StareTime < 50f ? 50 - (int)StareTime : 0;
            if (watched && StareTime < 50f) return;

            float speed = Mathf.Min(2f + (Time.time - StartTime) * 0.005f, 3.5f);
            if (StareTime >= 50f) speed = Mathf.Lerp(0.3f, speed, (StareTime - 50f) / 5f);

            Vector3 direction = GoalPosition - AngelPosition;
            direction.y = 0f;
            if (direction.magnitude > 0.05f) GhostYaw = Quaternion.LookRotation(direction).eulerAngles.y;
            AngelPosition = Vector3.MoveTowards(AngelPosition, GoalPosition, speed * Time.deltaTime);

            foreach (PlayerState player in GameReferences.Spawn.ActivePlayerStates)
            {
                if (!player.IsAlive || player.IsSpectating) continue;
                if (Vector3.Distance(AngelPosition, player.LocomotionPlayer.RigidbodyPosition) > 0.8f) continue;

                GameReferences.Killing.KillPlayer(GameReferences.Peer, player, player.PlayerId, false);
                MelonLogger.Msg($"Weeping Ghost killed {player.NetworkName.Value}");
                NextThink = 0f;
            }
        }

        private static bool Watched()
        {
            foreach (PlayerState player in GameReferences.Spawn.ActivePlayerStates)
            {
                if (!player.IsAlive || player.IsSpectating) continue;

                Vector3 eye = player.LocomotionPlayer.RigidbodyPosition + Vector3.up * 1.3f;
                Vector3 look = Quaternion.Euler(player.LocomotionPlayer.LocalCameraPitchAngle, player.LocomotionPlayer.RigidbodyRotation.eulerAngles.y, 0f) * Vector3.forward;
                if (Vector3.Angle(look, AngelPosition + Vector3.up - eye) < 55f && !Physics.Linecast(eye, AngelPosition + Vector3.up, mask, QueryTriggerInteraction.Ignore))
                    return true;
            }
            return false;
        }

        private static bool CanWalk(Vector3 from, Vector3 to)
        {
            if (Mathf.Abs(to.y - from.y) > 0.5f) return false;

            foreach (RaycastHit hit in Physics.SphereCastAll(from + Vector3.up * 0.6f, 0.25f, (to - from).normalized, Vector3.Distance(from, to), mask, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<NetworkedLocomotionPlayer>() == null && hit.collider.GetComponentInParent<XRRig>() == null)
                    return false;

            return true;
        }

        private static void CheckLights()
        {
            if (GameReferences.Sabotage == null || GameReferences.Sabotage.IsSabotageActive) return;

            GameReferences.Sabotage.RPC_SendSabotageToAll((int)SabotageType.Lights, -1);
        }
    }
}
