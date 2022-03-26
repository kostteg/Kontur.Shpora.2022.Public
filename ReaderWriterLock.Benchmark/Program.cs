using BenchmarkDotNet.Running;
using ReaderWriterLock.Benchmark;

var summary = BenchmarkRunner.Run<LockBenchmark>();
