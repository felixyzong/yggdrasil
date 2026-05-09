using System;
using Core.Battle.Commands;

namespace Core.Battle
{
    public class BattleCommandService
    {
        public CommandResult TryExecute(BattleState state, IBattleCommand command)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (command == null) throw new ArgumentNullException(nameof(command));

            var validateResult = command.Validate(state);
            if (!validateResult.IsSuccess)
            {
                return validateResult;
            }

            state.SetPhase(BattlePhase.ResolvingCommand);
            var executeResult = command.Execute(state);

            if (state.Phase == BattlePhase.ResolvingCommand)
            {
                state.SetPhase(BattlePhase.WaitingForCommand);
            }

            return executeResult;
        }
    }
}
