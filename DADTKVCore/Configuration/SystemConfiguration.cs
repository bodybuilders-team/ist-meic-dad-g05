namespace DADTKV;

public class SystemConfiguration
{
    public List<ProcessInfo> Processes { get; } = new();
    private static int _serverProcessesCount;

    public List<ProcessInfo> ServerProcesses => Processes.GetRange(0, _serverProcessesCount);

    public List<ProcessInfo> LeaseManagers
    {
        get { return Processes.FindAll(p => p.Role is "L"); }
    }

    public List<ProcessInfo> TransactionManagers
    {
        get { return Processes.FindAll(p => p.Role is "T"); }
    }

    private int Slots { get; set; }
    private int Duration { get; set; }
    private DateTime WallTime { get; set; }

    private Dictionary<int, List<Tuple<string, string>>> Suspicions { get; } = new();

    public IEnumerable<Tuple<string, string>> CurrentSuspicions
    {
        get
        {
            var currentTimeSlot = ((int)Math.Floor((DateTime.Now - WallTime).TotalMilliseconds / Duration)) + 1;
            return Suspicions[currentTimeSlot];
        }
    }

    public static SystemConfiguration? ReadSystemConfiguration(string filePath)
    {
        try
        {
            // Read and parse the configuration file
            var lines = File.ReadAllLines(filePath);
            var systemConfig = new SystemConfiguration();

            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                if (parts.Length >= 2)
                {
                    var command = parts[0];
                    var parameters = parts.Skip(1).ToArray();

                    List<ProcessInfo>? serverProcesses = null;

                    // Process different commands from the configuration file
                    switch (command)
                    {
                        case "#":
                            continue;
                        case "P": // Process identifier and role (Server or Client)
                            var process = new ProcessInfo
                            {
                                Id = parameters[0],
                                Role = parameters[1]
                            };

                            if (parameters.Length > 2)
                            {
                                process.URL = parameters[2];
                                _serverProcessesCount++;
                            }

                            systemConfig.Processes.Add(process);
                            break;

                        case "S": // Number of time slots
                            systemConfig.Slots = int.Parse(parameters[0]);
                            break;

                        case "D": // Duration of time slots in milliseconds
                            systemConfig.Duration = int.Parse(parameters[0]);
                            break;

                        case "T": // Global wall time of the first slot
                            systemConfig.WallTime = DateTime.Parse(parameters[0]);
                            break;

                        case "F": // Suspected nodes during time slots
                            var slotNumber = int.Parse(parameters[0]);
                            serverProcesses ??= systemConfig.ServerProcesses;
                            for (var i = 0; i < serverProcesses.Count; i++)
                            {
                                serverProcesses[i].SlotStatus[slotNumber] = parameters[1 + i];
                            }

                            var suspicions = parameters.Skip(1 + serverProcesses.Count).ToList();

                            systemConfig.Suspicions[slotNumber] = suspicions.Select(s => s.Trim('(', ')'))
                                .Select(s => new Tuple<string, string>(s.Split(',')[0], s.Split(',')[1])).ToList();
                            break;
                    }
                }
            }

            return systemConfig;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading system configuration: {ex.Message}");
            return null;
        }
    }
}