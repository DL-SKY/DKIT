namespace Modules.Match3.Scripts.Interfaces
{
    public interface IGameRoundData
    {
        int[,] GetMask();
        int[,] GetPresets();
    }
}
