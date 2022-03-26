using BenchmarkDotNet.Attributes;

namespace ReaderWriterLock.Benchmark
{
    [MemoryDiagnoser]
    public class LockBenchmark
    {
        [Params(1000)]
        public int ReadCounts;

        [Params(
            typeof(ReaderWriterLockWrapper)/*,
            typeof(RwLockStraightForward),
            typeof(RwLockAutoResetEvent),
            typeof(RwLockAutoResetEvent2),
            typeof(RwLockPulseWait),
            typeof(RwLockInterlockedSpinWait),
            typeof(RwLockInterlockedSpinWait2),
            typeof(RwLockPulseWaitAndInterlocked)*/)]
        public Type Implementation;

        private Action[] actions;
        private IRwLock rwLock;

        [GlobalSetup]
        public void SetUp()
        {
            actions = new Action[ReadCounts];
            for (var i = 0; i < ReadCounts; i++)
                actions[i] = () => Thread.SpinWait(10);
            rwLock = (IRwLock)Activator.CreateInstance(Implementation);
        }

        [Benchmark]
        public void Readers()
        {
            actions.AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .ForAll(x => rwLock.ReadLocked(x));
        }

        [Benchmark]
        public void Writers()
        {
            actions.AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .ForAll(x => rwLock.WriteLocked(x));
        }

        [Benchmark]
        public void MultipleExecution()
        {
            var current = 0;
            actions.AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .ForAll(x =>
            {
                Interlocked.Increment(ref current);
                if (current % 50 == 0)
                    rwLock.WriteLocked(x);
                else
                    rwLock.ReadLocked(x);
            });
        }
    }
}