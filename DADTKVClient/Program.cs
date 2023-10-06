﻿namespace DADTKV;

internal static class Program
{
    // Entry point for the client application
    // Arguments: serverUrl clientID scriptFilePath
    public static void Main(string[] args)
    {
        if (args.Length != 3)
            throw new ArgumentException("Invalid arguments.");

        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        // Client configuration
        var serverUrl = args[0];
        var clientId = args[1];
        var clientLogic = new ClientLogic(clientId, serverUrl);

        // Script configuration
        var scriptFilePath = Path.Combine(Environment.CurrentDirectory, args[2]);
        var scriptReader = new ScriptReader(File.ReadAllText(scriptFilePath));

        while (scriptReader.HasNextCommand())
        {
            var command = scriptReader.NextCommand();

            lock (clientLogic)
            {
                switch (command)
                {
                    case TransactionCommand transactionCommand:
                        var writeSet = transactionCommand.WriteSet
                            .Select(x => new DadInt
                            {
                                Key = x.Key,
                                Value = x.Value
                            }).ToList();

                        var readSet = clientLogic.TxSubmit(transactionCommand.ReadSet.ToList(), writeSet)
                            .Result;
                        Console.WriteLine("Read set: " + readSet);
                        break;
                    case WaitCommand waitCommand:
                        Console.WriteLine("Waiting " + waitCommand.Milliseconds + " milliseconds");
                        Thread.Sleep(waitCommand.Milliseconds);
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
        }
    }
}