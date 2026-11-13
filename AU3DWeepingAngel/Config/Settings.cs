namespace AU3DWeepingAngel.Config
{
    internal class Settings
    {
        internal static bool InGame = false;
        internal static bool WeepingAngelEnabled = true;
        internal static bool IsHost => GameReferences.Rig != null && GameReferences.Rig.PState.PlayerId == 9;
    }
}
