namespace Dadtkv;

public class LearnRequest : ITobRequest<LearnRequest>
{
    private readonly ProcessConfiguration _processConfiguration;
    public ulong RoundNumber { get; }
    public ConsensusValue ConsensusValue { get; }
    public ulong ServerId { get; }
    public ulong SequenceNum { get; set; }
    public ulong TobMessageId { get; set; }
    public ulong UrbMessageId => ServerId + SequenceNum * (ulong)_processConfiguration.ServerProcesses.Count;

    public LearnRequest(ProcessConfiguration processConfiguration, ulong serverId, ulong roundNumber,
        ConsensusValue consensusValue, ulong sequenceNum = 0)
    {
        _processConfiguration = processConfiguration;
        ServerId = serverId;
        SequenceNum = sequenceNum;
        RoundNumber = roundNumber;
        ConsensusValue = consensusValue;
    }

    public int CompareTo(LearnRequest? other)
    {
        return TobMessageId.CompareTo(other?.TobMessageId);
    }
}