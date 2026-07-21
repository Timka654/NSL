namespace NSL.ShaderVM
{
    [ShaderIgnore]
    public interface IExecutionContext
    {
        uint GlobalInvocationIdX { get; }

        uint GlobalInvocationIdY { get; }

        uint GlobalInvocationIdZ { get; }

        uint LocalInvocationIdX { get; }

        uint LocalInvocationIdY { get; }

        uint LocalInvocationIdZ { get; }

        uint WorkGroupIdX { get; }

        uint WorkGroupIdY { get; }

        uint WorkGroupIdZ { get; }

        uint LocalSizeX { get; }

        uint LocalSizeY { get; }

        uint LocalSizeZ { get; }

        void Barrier();

        void MemoryBarrier();

        void MemoryBarrierShared();

        void LogTrace(string message);
    }
}
