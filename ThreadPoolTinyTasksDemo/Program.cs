using System.Diagnostics;

namespace ThreadPoolTinyTasksDemo
{
    class Program
    {
        static int X;

        static void Main(string[] args)
        {
            // while (true)
            // {
            //     Task.Run(() => X += 1).Wait();
            // }

            var testParams = new (int tasks, int actions)[] { (10000000, 1), (1000000, 10), (100000, 100), (10000, 1000) };
            foreach(var (tsks, acts) in testParams)
            {
                X = 0;
                TasksCount = tsks;
                ActionsInTask = acts;

                var sw = Stopwatch.StartNew();
                RunOneTaskSequentially();
                sw.Stop();
                Console.WriteLine($"Elapsed {sw.Elapsed}. X = {X}. TasksCount = {TasksCount}, ActionsInTask = {ActionsInTask}.");
            }
        }


        static int TasksCount;
        static int ActionsInTask;
        public static void RunOneTaskSequentially()
        {
            for (var i = 0; i < TasksCount; i++)
            {
                Task.Run(() => ChangeFunction()).Wait();
            }
        }

        private static Action ChangeFunction = () =>
        {
            for (int i = 0; i < ActionsInTask; i++)
                X++;
        };
    }
}