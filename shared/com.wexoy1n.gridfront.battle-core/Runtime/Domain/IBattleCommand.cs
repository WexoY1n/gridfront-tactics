namespace Gridfront.BattleCore.Domain
{
    /// <summary>
    /// Player or system intent applied at a specific battle tick.
    /// </summary>
    public interface IBattleCommand
    {
        int ExecuteAtTick { get; }

        long Sequence { get; }
    }
}
