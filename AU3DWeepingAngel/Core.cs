using MelonLoader;
using AU3DWeepingAngel.Config;
using AU3DWeepingAngel.Managers;

[assembly: MelonInfo(typeof(AU3DWeepingAngel.Core), "AU3DWeepingAngel", "1.0.0", "Jolyne", "")]
[assembly: MelonColor(0, 40, 40, 40)]
[assembly: MelonGame("Schell Games", null)]
namespace AU3DWeepingAngel
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("AU3DWeepingAngel Loaded!");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            Settings.InGame = sceneName != "Boot" && sceneName != "Title";
            GhostManager.Active = false;
        }

        public override void OnUpdate()
        {
            if (!Settings.InGame) return;

            if (GameReferences.Spawn == null || GameReferences.Session == null)
                GameReferences.ResetReferences();

            if (GameReferences.Spawn == null || GameReferences.Session == null || GameReferences.GameState == null || GameReferences.Runner == null) return;
            if (GameReferences.Rig == null || GameReferences.Rig.PState == null || GameReferences.Rig.PState.LocomotionPlayer == null) return;

            bool inRound = GameReferences.GameState.InTaskState() && GameReferences.Allowed;
            if (inRound && Settings.IsHost) GhostManager.Update();
            else if (GhostManager.Active && !GameReferences.GameState.InVotingState()) GhostManager.Active = false;

            RenderManager.Update(inRound);
        }
    }
}
