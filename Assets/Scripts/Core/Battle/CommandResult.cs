using System;
using System.Collections.Generic;
using Core.Battle.Events;

namespace Core.Battle
{
    public class CommandResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public IReadOnlyList<IBattleEvent> Events { get; }

        private CommandResult(bool isSuccess, string errorMessage, IReadOnlyList<IBattleEvent> events)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Events = events;
        }

        public static CommandResult Success(params IBattleEvent[] events)
        {
            return new CommandResult(true, string.Empty, new List<IBattleEvent>(events ?? Array.Empty<IBattleEvent>()).AsReadOnly());
        }

        public static CommandResult Success(IEnumerable<IBattleEvent> events)
        {
            return new CommandResult(true, string.Empty, new List<IBattleEvent>(events ?? Array.Empty<IBattleEvent>()).AsReadOnly());
        }

        public static CommandResult Failure(string errorMessage)
        {
            return new CommandResult(false, errorMessage ?? string.Empty, Array.Empty<IBattleEvent>());
        }
    }
}
