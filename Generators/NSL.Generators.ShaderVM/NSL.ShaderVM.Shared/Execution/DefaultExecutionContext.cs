using System;
using System.Diagnostics;

namespace NSL.ShaderVM
{
    [ShaderIgnore]
    public class DefaultExecutionContext : IExecutionContext
    {
        private float[] _globalMemory;

        private float[] _localSnapshot;

        private float[] _sharedMemory;

        private readonly object _lock = new object();


        public virtual uint GlobalInvocationIdX { get; set; }

        public virtual uint GlobalInvocationIdY { get; set; }

        public virtual uint GlobalInvocationIdZ { get; set; }

        public virtual uint LocalInvocationIdX { get; set; }

        public virtual uint LocalInvocationIdY { get; set; }

        public virtual uint LocalInvocationIdZ { get; set; }

        public virtual uint WorkGroupIdX { get; set; }

        public virtual uint WorkGroupIdY { get; set; }

        public virtual uint WorkGroupIdZ { get; set; }

        public virtual uint LocalSizeX { get; set; } = 1;

        public virtual uint LocalSizeY { get; set; } = 1;

        public virtual uint LocalSizeZ { get; set; } = 1;

        public DefaultExecutionContext(uint globalSize)
        {
            _globalMemory = new float[globalSize];

            _localSnapshot = new float[globalSize];
        }

        public DefaultExecutionContext(float[] globalMemory, float[] sharedMemory = null)
        {
            _globalMemory = globalMemory ?? throw new ArgumentNullException(nameof(globalMemory));

            _localSnapshot = new float[globalMemory.Length];

            _sharedMemory = sharedMemory;
        }

        public virtual void Barrier()
        {
            MemoryBarrier();
        }

        public virtual void MemoryBarrier()
        {
            lock (_lock)
            {
                for (int i = 0; i < _globalMemory.Length; i++)
                    _globalMemory[i] = _localSnapshot[i];
            }
        }

        public virtual void MemoryBarrierShared()
        {
            Barrier();
        }

        public virtual void LogTrace(string message)
        {
            Debug.WriteLine($"[NSL.ShaderVM|{GlobalInvocationIdX},{GlobalInvocationIdY}] {message}");
        }

        public void CopyFrom(float[] source)
        {
            for (uint i = 0; i < source.Length && i < _localSnapshot.Length; i++)
                _localSnapshot[i] = source[i];
        }
        public void CopyTo(float[] target)
        {
            lock (_lock)
            {
                for (int i = 0; i < target.Length && i < _globalMemory.Length; i++)
                    target[i] = _globalMemory[i];
            }
        }

        public float[] GetGlobalMemory()
            => _globalMemory;
        
        public float[] GetLocalSnapshot()
            => _localSnapshot;
    }
}
