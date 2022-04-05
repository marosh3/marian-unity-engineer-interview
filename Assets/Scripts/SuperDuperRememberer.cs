using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Superscale
{
    public enum CommandStatus {
        RUNNING,
        DONE,
        FAILED
    }

    public readonly struct CommandState {
        public readonly string id;
        public readonly CommandStatus status;
        public readonly string error;

        public CommandState(string id, CommandStatus status, string error)
        {
            this.id = id;
            this.status = status;
            this.error = error;
        }
    }

    public interface Command {}

    public class ExecuteStatementCommand : Command
    {
        public readonly string statement;

        public ExecuteStatementCommand(string statement)
        {
            this.statement = statement;
        }
    }

    public class DescribeStatementCommand : Command
    {
        public readonly string id;

        public DescribeStatementCommand(string id)
        {
            this.id = id;
        }
    }

    public class SuperDuperRememberer
    {
        private object _lock = new object();

        private List<string> rememberedItems;
        private System.Random randomGenerator;
        private Dictionary<string, CommandState> commandState;
        private HashSet<int> allIDs = new HashSet<int>();

        public string[] Items {
            get {
                lock (_lock)
                {
                    return rememberedItems.ToArray();
                }
            }
        }

        public SuperDuperRememberer()
        {
            rememberedItems = new List<string>();
            randomGenerator = new System.Random();
            commandState = new Dictionary<string, CommandState>();
        }

        private string GenerateID()
        {
            while (true)
            {
                var id_num = randomGenerator.Next(999999999);

                if (allIDs.Contains(id_num))
                {
                    Debug.LogWarning("duplicit ID, trying another");
                    continue;
                }

                allIDs.Add(id_num);

                var id_str = id_num.ToString();

                return id_str;
            }
        }

        public CommandState ExecuteCommand(Command command)
        {
            if (command is ExecuteStatementCommand)
            {
                var commandId = GenerateID();

                this.commandState[commandId] = new CommandState(commandId, CommandStatus.RUNNING, null);
                Task.Run(async () => {
                    await Task.Delay(500 + randomGenerator.Next(500));
                    try
                    {
                        this.ExecuteStatement(commandId, (command as ExecuteStatementCommand).statement);
                        this.commandState[commandId] = new CommandState(commandId, CommandStatus.DONE, null);
                    } catch (Exception e)
                    {
                        this.commandState[commandId] = new CommandState(commandId, CommandStatus.FAILED, e.Message);
                    }
                });
                return this.commandState[commandId];
            }
            else if (command is DescribeStatementCommand)
            {
                var commandId = (command as DescribeStatementCommand).id;
                if (!this.commandState.ContainsKey(commandId)) {
                    throw new Exception($"COMMAND {commandId} NOT FOUND!");
                }
                return this.commandState[commandId];
            }
            else
            {
                throw new Exception("Unknown command!");
            }
        }

        private void ExecuteStatement(string commandId, string statement)
        {
            if (statement.Length > 300) {
                throw new Exception("THERE'S NO WAY TO REMEMBER ALL THAT AT ONCE!");
            }

            var regex = new Regex(@"^CAN YOU PLEASE REMEMBER THESE ITEMS\? ('[a-z]+')(,'[a-z]+')* K THX BYE.$");

            if (!regex.IsMatch(statement)) {
                throw new Exception("STATEMENT IS INVALID");
            }

            foreach (Match match in new Regex(@"'([a-z]+)'").Matches(statement))
            {
                lock (_lock)
                {
                    this.rememberedItems.Add(match.Value.Replace("'", ""));
                }
            }
        }
    }
}