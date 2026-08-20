namespace Gridfront.BattleCore.Combat
{
    public enum DeployReject : byte
    {
        None = 0,
        OutOfBounds = 1,
        Occupied = 2,
        WrongTile = 3,
        InsufficientCost = 4
    }
}
