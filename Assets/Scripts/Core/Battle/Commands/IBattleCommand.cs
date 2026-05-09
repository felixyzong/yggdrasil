namespace Core.Battle.Commands
{
    public interface IBattleCommand
    {
        CommandResult Validate(BattleState state);
        CommandResult Execute(BattleState state);
    }
}
