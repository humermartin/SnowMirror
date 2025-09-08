namespace MirrorRepository.Enums
{
    public enum EnumSyncProcessState
    {
        NotStarted = 1,
        Running = 2,
        Finished = 3,
        Interrupted = 4,
        FinalError = 5,
        Canceled = 6,
        Suspended = 7,
        Continue = 8
    }
}