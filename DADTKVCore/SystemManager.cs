﻿using System.Diagnostics;

namespace Dadtkv;

/// <summary>
///     The system manager is responsible for starting the Dadtkv system.
/// </summary>
internal class SystemManager
{
    private readonly List<Process> _processes = new();

    /// <summary>
    ///     Starts the Dadtkv servers (Transaction Managers, Lease Managers).
    /// </summary>
    /// <param name="config">The system configuration.</param>
    /// <param name="configurationFile">The system configuration file.</param>
    public void StartServers(SystemConfiguration config, string configurationFile)
    {
        var lms = config.ServerProcesses.Where(process => process.Role == "L").ToList();
        var tms = config.ServerProcesses.Where(process => process.Role == "T").ToList();

        StartServers(lms, configurationFile);
        Thread.Sleep(1000);
        StartServers(tms, configurationFile);
    }

    private void StartServers(List<ProcessInfo> serverProcesses, string configurationFile)
    {
        var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.Parent!.FullName;
        var leaseManagerExePath =
            Path.Combine(solutionDirectory, "DadtkvLeaseManager/bin/Debug/net6.0/DadtkvLeaseManager.exe");
        var transactionManagerExePath = Path.Combine(solutionDirectory,
            "DadtkvTransactionManager/bin/Debug/net6.0/DadtkvTransactionManager.exe");

        foreach (var process in serverProcesses)
        {
            Console.WriteLine($"Starting {process.Role} {process.Id} at {process.Url}");
            var fileName = process.Role == "L" ? leaseManagerExePath : transactionManagerExePath;

            var p = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                ArgumentList = { process.Id, configurationFile }
            }) ?? throw new Exception("Failed to start server process: " + process.Id);
            _processes.Add(p);
        }
    }

    /// <summary>
    ///     Starts the Dadtkv clients.
    /// </summary>
    /// <param name="config">The system configuration.</param>
    public void StartClients(SystemConfiguration config)
    {
        var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.Parent!.FullName;
        var clientExePath = Path.Combine(solutionDirectory, "DadtkvClient/bin/Debug/net6.0/DadtkvClient.exe");
        var clientScriptsDirectory = Path.Combine(solutionDirectory, "DadtkvClient/Script");
        var clientScriptFiles = Directory.GetFiles(clientScriptsDirectory, "*.txt");

        foreach (var client in config.Clients)
        {
            Console.WriteLine($"Starting client {client.Id}");

            var p = Process.Start(new ProcessStartInfo
            {
                FileName = clientExePath,
                ArgumentList =
                {
                    config.TransactionManagers[new Random().Next(config.TransactionManagers.Count)].Url!,
                    client.Id,
                    clientScriptFiles[new Random().Next(clientScriptFiles.Length)]
                }
            }) ?? throw new Exception("Failed to start client process: " + client.Id);
            _processes.Add(p);
        }
    }

    /// <summary>
    ///     Shuts down the Dadtkv system.
    /// </summary>
    public void ShutDown()
    {
        foreach (var process in _processes)
            process.Kill();
    }

    /// <summary>
    ///     Checks if the Dadtkv system is running.
    /// </summary>
    /// <returns>True if the Dadtkv system is running, false otherwise.</returns>
    public bool IsRunning()
    {
        return _processes.All(process => !process.HasExited);
    }
}