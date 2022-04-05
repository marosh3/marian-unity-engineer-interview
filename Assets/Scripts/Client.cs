using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Superscale {
    public class Client {

        public static readonly string E_WORD_TOO_LONG = "word is too long";
        public static readonly string E_FAILED_COMMAND = "command failed";

        private SuperDuperRememberer rememberer;

        private List<string> pendingDataIDs = new List<string>();

        private StringBuilder cache = new StringBuilder(300);

        private const int totalAllowedLength = 300;

        private const string statementBegin = "CAN YOU PLEASE REMEMBER THESE ITEMS? ";
        private const string statementEnd = " K THX BYE.";

        private readonly int statementBeginLength = statementBegin.Length;
        private readonly int statementEndLength = statementEnd.Length;

        public Client(SuperDuperRememberer rememberer)
        {
            this.rememberer = rememberer;
            cache.Append(statementBegin);
        }

        public void RememberData(string[] words)
        {
            int wordsLength = words.Length;

            for (int i = 0; i < wordsLength; i++)
            {
                string word = words[i];
                int wordLengthQuoted = word.Length + 2;

                if (statementBeginLength + wordLengthQuoted + statementEndLength > totalAllowedLength)
                    throw new System.Exception(E_WORD_TOO_LONG);

                if (cache.Length > statementBeginLength)
                {
                    if (cache.Length + 1 + wordLengthQuoted + statementEndLength > totalAllowedLength)
                        FlushCache();
                    else
                        cache.Append(",");
                }

                cache.Append("'");
                cache.Append(word);
                cache.Append("'");
            }
        }

        public void FlushCache()
        {
            if (cache.Length == statementBeginLength) return;

            cache.Append(statementEnd);
            string statementCommand = cache.ToString();

            cache.Clear();
            cache.Append(statementBegin);

            var executeState = rememberer.ExecuteCommand(new ExecuteStatementCommand(statementCommand));

            switch (executeState.status)
            {
                case CommandStatus.RUNNING:
                    pendingDataIDs.Add(executeState.id);
                    break;
                case CommandStatus.FAILED:
                    throw new System.Exception(E_FAILED_COMMAND);
            }
        }

        public IEnumerator WaitForPendingDataCoroutine()
        {
            while (true)
            {
                int count = pendingDataIDs.Count;
                if (count == 0) break;

                string id = pendingDataIDs[0];

                while (true)
                {
                    bool done = false;
                    var describeState = rememberer.ExecuteCommand(new DescribeStatementCommand(id));
                    switch (describeState.status)
                    {
                        case CommandStatus.DONE: 
                            done = true; 
                            break;
                        case CommandStatus.FAILED:
                            throw new System.Exception(E_FAILED_COMMAND);
                    }
                    if (done) break;
                    yield return new WaitForSeconds(.1f);
                }

                pendingDataIDs.RemoveAt(0);
            }
        }
    }
}