public enum MovementType
{
    Line,
    Diagonal,
    LineAndDiagonal
}

public enum AttackType
{
    Line,
    Diagonal,
    LineAndDiagonal
}

public enum User
{
    Player,
    Enemy,
    Neutral
}

public enum TimerType
{
    GameTimer,
    TurnTimer
}

public enum Unit
{
    Prawn,
    Building,
    King,
    Knight,
    Gobbo,
    Tree,
    Resource
}

public enum TurnState
{
    WaitForInput,
    WaitForEnemyInput,
    ProcessMyTurn,
    ProcessEnemyTurn
}

public enum GameType
{
    Replay,
    Live
}