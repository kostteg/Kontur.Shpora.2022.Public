using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace ReaderWriterLock
{
	//public class LockTests : RwLockTests<LockWrapper> {}
	public class ReaderWriterLockSlimTests : RwLockTests<ReaderWriterLockWrapper> {}

	//public class RwLockTests : RwLockTests<RwLock> {}

	[TestFixture]
	public abstract class RwLockTests<T> where T : IRwLock, new()
	{
		[SetUp]
		public void SetUp()
		{
			readers = writers = reads = writes = 0;
			writeEvent.Reset();
		}

		[TestCase(1000)]
		public void TestMultipleReadsWithoutWriter(int readersCount)
		{
			var threads = Enumerable.Range(0, readersCount).Select(_ => new Thread(() => Read(3000))).ToArray();
			threads.ForEach(thread => thread.Start());

			Thread.Sleep(1500);
			Assert.AreEqual(readersCount, readers);

			threads.ForEach(thread => thread.Join());
			Assert.AreEqual(0, readers);
		}

		[TestCase(100)]
		public void TestMultipleWritersAreSerialized(int writersCount)
		{
			var threads = Enumerable.Range(0, writersCount).Select(_ => new Thread(() => Read(100))).ToArray();
			threads.ForEach(thread => thread.Start());
			threads.ForEach(thread => thread.Join());
		}

		[TestCase(1000)]
		public void TestNoSleepWhileAwaitingLock(int iterations)
		{
			var readThread = new Thread(_ => Enumerable.Range(0, iterations).ForEach(_ => Read(0)));
			var writeThread = new Thread(_ => Enumerable.Range(0, iterations).ForEach(_ => Write(0)));

			readThread.Start();
			writeThread.Start();

			var stopwatch = Stopwatch.StartNew();
			readThread.Join();
			writeThread.Join();
			stopwatch.Stop();

			Console.WriteLine(stopwatch.Elapsed);
			Assert.Less(stopwatch.ElapsedMilliseconds, iterations / 2);
		}

		[Test]
		public void TestNoSpinWhileAwaitingLock()
		{
			var fast = new[] { new Thread(_ => Read(1)), new Thread(_ => Write(1)) };
			var slow = new[] { new Thread(_ => Read(4000)), new Thread(_ => Write(4000)) };

			var process = Process.GetCurrentProcess();

			slow.ForEach(thread => thread.Start());
			Thread.Sleep(1000);
			fast.ForEach(thread => thread.Start());

			var sw = Stopwatch.StartNew();
			var start = process.TotalProcessorTime;
			fast.ForEach(thread => thread.Join());
			var end = process.TotalProcessorTime;
			sw.Stop();

			var cpuUsage = (end - start).TotalMilliseconds / (Environment.ProcessorCount * sw.ElapsedMilliseconds);
			Console.WriteLine("CPU Usage awaiting lock: " + cpuUsage.ToString("P"));

			Assert.Less(cpuUsage, 0.5 / Environment.ProcessorCount);
		}

		[TestCase(10, 100, 0, 0, 0, 0)]
		[TestCase(100, 10, 0, 0, 0, 0)]
		[TestCase(10, 100, 0, 0, 1, 1)]
		[TestCase(100, 10, 0, 0, 1, 1)]
		[TestCase(10, 2, 0, 1, 0, 0)]
		[TestCase(10, 2, 0, 0, 0, 1)]
		public void TestStress(int readersCount, int writersCount, int readPeriod, int writePeriod, int readTimeout, int writeTimeout)
		{
			using var cts = new CancellationTokenSource(10000);

			var readThreads = Enumerable.Range(0, readersCount).Select(_ => new Thread(_ => ReadIndefinitely(cts.Token, readPeriod, readTimeout))).ToArray();
			var writeThreads = Enumerable.Range(0, writersCount).Select(_ => new Thread(_ => WriteIndefinitely(cts.Token, writePeriod, writeTimeout))).ToArray();

			readThreads.ForEach(thread => thread.Start());
			writeThreads.ForEach(thread => thread.Start());

			readThreads.ForEach(thread => thread.Join());
			writeThreads.ForEach(thread => thread.Join());

			Console.WriteLine("Reads: " + reads);
			Console.WriteLine("Writes: " + writes);
		}

		private static void ReadIndefinitely(CancellationToken cancellationToken, int period, int timeout)
		{
			while(!cancellationToken.IsCancellationRequested)
			{
				Read(timeout);
				Thread.Sleep(period);
			}
		}

		private static void WriteIndefinitely(CancellationToken cancellationToken, int period, int timeout)
		{
			while(!cancellationToken.IsCancellationRequested)
			{
				Write(timeout);
				Thread.Sleep(period);
			}
		}

		private static void Read(int timeout)
			=> rwLock.ReadLocked(() =>
			{
				Interlocked.Increment(ref readers);

				if(writers > 0 || writeEvent.Wait(timeout))
					throw new Exception("Readers not allowed when write lock taken");

				Interlocked.Decrement(ref readers);

				Interlocked.Increment(ref reads);
			});

		private static void Write(int timeout)
			=> rwLock.WriteLocked(() =>
			{
				if(readers > 0)
					throw new Exception("Readers not allowed when write lock taken");

				writeEvent.Set();

				if(Interlocked.Increment(ref writers) > 1)
					throw new Exception("Only one writer allowed");

				Thread.Sleep(timeout);
				//Thread.SpinWait((timeout + 1) * 100);

				if(Interlocked.Decrement(ref writers) != 0)
					throw new Exception("Only one writer allowed");

				writeEvent.Reset();

				Interlocked.Increment(ref writes);
			});

		private static readonly IRwLock rwLock = new T();

		private static readonly ManualResetEventSlim writeEvent = new ManualResetEventSlim(false);

		private static volatile int readers, reads;
		private static volatile int writers, writes;
	}

	public static class Extension
	{
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach(var item in enumerable)
				action(item);
		}
	}
}
