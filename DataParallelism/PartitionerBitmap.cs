using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DataParallelism
{
	static class PartitionerBitmap
	{
		private const int DegreeOfParallelizm = 3;

		public static void BitmapDemo()
		{
			int width = 128;
			int height = 128;

			var local = new ThreadLocal<int>(() => Guid.NewGuid().GetHashCode() | unchecked((int)0xff000000));

			var matrix = new int[height, width];
			var sum =
			// ===========================================
				Enumerable.Range(0, width * height).AsParallel()
			// ===========================================
			// ParallelEnumerable.Range(0, width * height)
			// ===========================================
				.WithDegreeOfParallelism(DegreeOfParallelizm)
			// ===========================================
				.GroupBy(i => i % DegreeOfParallelizm) // hash partitioning
				.SelectMany(g => g)
			// ===========================================
				.Select(i =>
				{
					Thread.SpinWait(GetSleepTime());
					matrix[i / width, i % width] = local.Value;
					return i;
				})
				.Sum(i => (long)i);

			var bmp = new Bitmap(width, height);
			for(int y = 0; y < height; y++)
			for(int x = 0; x < width; x++)
				bmp.SetPixel(x, y, Color.FromArgb(matrix[y, x]));

			bmp.Save("qqq.png");
			Console.WriteLine(sum);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetSleepTime()
			=> 100;
			//=> 100 * (Thread.CurrentThread.ManagedThreadId % 4);
	}
}
