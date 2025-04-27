using System.Diagnostics;
using System.Text;

namespace Watki
{
    internal class Program
    {
        static double MeasureAverageTime(int repetitions, Action action)
        {
            long totalMs = 0;
            for (int i = 0; i < repetitions; i++)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                totalMs += sw.ElapsedMilliseconds;
            }
            return totalMs / (double)repetitions;
        }
        static void Main(string[] args)
        {
            MatrixMultiplier matrixMultiplier = new MatrixMultiplier();
            var csvPath = "results.csv";
            var sb = new StringBuilder();
            sb.AppendLine("Metoda,Rozmiar,LiczbaWatkow,SredniCzasMs");

            int[] sizes = { 200, 500, 1000 }; // rozmiary macierzy
            int[] threadCounts = { 2, 4, 6, 8, 16, 32 }; // liczby wątków
            int repetitions = 5; // liczba powtórzeń

            foreach (var size in sizes)
            {
                Console.WriteLine($"\n--- Rozmiar macierzy: {size}x{size} ---");
                double[,] A = matrixMultiplier.GenerateMatrix(size);
                double[,] B = matrixMultiplier.GenerateMatrix(size);

                // Sekwencyjnie
                double avgSequential = MeasureAverageTime(repetitions, () =>
                {
                    double[,] resultSeq = new double[size, size];
                    matrixMultiplier.MultiplySequential(A, B, resultSeq, size);
                });
                Console.WriteLine($"Sekwencyjne: {avgSequential} ms");
                sb.AppendLine($"Sekwencyjne,{size},1,{avgSequential}");

                foreach (var threads in threadCounts)
                {
                    // Parallel.For
                    double avgParallel = MeasureAverageTime(repetitions, () =>
                    {
                        double[,] resultPar = new double[size, size];
                        matrixMultiplier.MultiplyParallel(A, B, resultPar, size, threads);
                    });
                    Console.WriteLine($"Parallel ({threads} watków): {avgParallel} ms");
                    sb.AppendLine($"Parallel,{size},{threads},{avgParallel}");

                    // Własne wątki
                    double avgThreads = MeasureAverageTime(repetitions, () =>
                    {
                        double[,] resultThread = new double[size, size];
                        matrixMultiplier.MultiplyWithThreads(A, B, resultThread, size, threads);
                    });
                    Console.WriteLine($"Threads ({threads} watków): {avgThreads} ms");
                    sb.AppendLine($"Threads,{size},{threads},{avgThreads}");
                }
            }

            File.WriteAllText(csvPath, sb.ToString());
            Console.WriteLine($"\nWyniki zapisane do {csvPath}");
        }
    }
}