using Il2CppTMPro;
using UnityEngine;
using AU3DWeepingAngel.Config;
using Object = UnityEngine.Object;

namespace AU3DWeepingAngel.Managers
{
    internal static class RenderManager
    {
        private static GameObject WeepingAngel;
        private static TextMeshPro CountdownText;
        private static Vector3 ShownPosition;

        internal static void Update(bool inRound)
        {
            if (!inRound || (Settings.IsHost ? !GhostManager.Active : Time.time - NetworkManager.LastUpdate > 1.5f))
            {
                if (WeepingAngel != null && WeepingAngel.activeSelf) WeepingAngel.SetActive(false);
                return;
            }

            if (WeepingAngel == null && !Create()) return;

            if (!WeepingAngel.activeSelf)
            {
                WeepingAngel.SetActive(true);
                ShownPosition = GhostManager.AngelPosition;
            }

            ShownPosition = Vector3.Lerp(ShownPosition, GhostManager.AngelPosition, Time.deltaTime * 15f);
            WeepingAngel.transform.SetPositionAndRotation(ShownPosition, Quaternion.Slerp(WeepingAngel.transform.rotation, Quaternion.Euler(0f, GhostManager.GhostYaw, 0f), Time.deltaTime * 15f));

            CountdownText.text = GhostManager.Countdown > 0 ? GhostManager.Countdown.ToString() : "";
            if (Camera.main != null) CountdownText.transform.rotation = Camera.main.transform.rotation;
        }

        private static bool Create()
        {
            foreach (SkinnedMeshRenderer skin in GameReferences.Rig.PState.LocomotionPlayer.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (skin.name != "SK_GhostHandless_01") continue;

                WeepingAngel = new GameObject("WeepingAngel");
                Object.DontDestroyOnLoad(WeepingAngel);

                GameObject body = new GameObject("Body");
                body.transform.SetParent(WeepingAngel.transform, false);
                body.transform.localPosition = Vector3.up * 0.1f;

                Mesh mesh = new Mesh();
                skin.BakeMesh(mesh);
                body.AddComponent<MeshFilter>().mesh = mesh;

                Material material = new Material(skin.sharedMaterial);
                material.SetFloat("_ColorID", 6f);
                body.AddComponent<MeshRenderer>().material = material;

                CountdownText = new GameObject("Countdown").AddComponent<TextMeshPro>();
                CountdownText.transform.SetParent(WeepingAngel.transform, false);
                CountdownText.transform.localPosition = Vector3.up * 2.3f;
                CountdownText.font = Object.FindObjectOfType<TMP_Text>(true)?.font;
                CountdownText.fontSize = 6f;
                CountdownText.alignment = TextAlignmentOptions.Center;
                CountdownText.color = Color.red;
                return true;
            }
            return false;
        }
    }
}
