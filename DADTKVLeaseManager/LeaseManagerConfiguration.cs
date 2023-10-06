namespace DADTKV;

/// <summary>
///     Configuration of a lease manager process.
/// </summary>
public class LeaseManagerConfiguration : ProcessConfiguration
{
    public LeaseManagerConfiguration(ProcessConfiguration processConfiguration) : base(
        processConfiguration, processConfiguration.ProcessInfo.Id)
    {
    }

    public IEnumerable<ProcessInfo> OtherLeaseManagers =>
        LeaseManagers.Where(info => info.Id != ProcessInfo.Id).ToList();

    /// <summary>
    ///     The lease manager with the lowest id is the leader.
    /// </summary>
    /// <returns>The id of the leader.</returns>
    private string? GetLeaderId()
    {
        return LeaseManagers
            .Where(lm => !MyCurrentSuspicions.Contains(lm.Id))
            .MinBy(info => GetLeaseManagerIdNum(info.Id))?.Id;
    }

    /// <summary>
    ///     Checks if this process is the leader.
    /// </summary>
    /// <returns>True if this process is the leader, false otherwise.</returns>
    public bool IsLeader()
    {
        return GetLeaderId() == ProcessInfo.Id;
    }
}