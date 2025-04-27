using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Watki
{
    class MatrixMultiplier
    {
        private int currentRow = 0;
        private object rowLock = new object();

        public double[,] GenerateMatrix(int size)
        {
            var rand = new Random();
            var matrix = new double[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    matrix[i, j] = rand.NextDouble() * 10;
            return matrix;
        }

        public void MultiplySequential(double[,] A, double[,] B, double[,] result, int size)
        {
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < size; k++)
                        sum += A[i, k] * B[k, j];
                    result[i, j] = sum;
                }
        }

        public void MultiplyParallel(double[,] A, double[,] B, double[,] result, int size, int maxThreads)
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxThreads };
            Parallel.For(0, size, options, i =>
            {
                for (int j = 0; j < size; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < size; k++)
                        sum += A[i, k] * B[k, j];
                    result[i, j] = sum;
                }
            });
        }

        public void MultiplyWithThreads(double[,] A, double[,] B, double[,] result, int size, int maxThreads)
        {
            currentRow = 0;
            Thread[] threads = new Thread[maxThreads];

            for (int t = 0; t < maxThreads; t++)
            {
                threads[t] = new Thread(() =>
                {
                    while (true)
                    {
                        int row;

                        lock (rowLock)
                        {
                            if (currentRow >= size)
                                break;
                            row = currentRow;
                            currentRow++;
                        }

                        for (int j = 0; j < size; j++)
                        {
                            double sum = 0;
                            for (int k = 0; k < size; k++)
                                sum += A[row, k] * B[k, j];
                            result[row, j] = sum;
                        }
                    }
                });

                threads[t].Start();
            }

            for (int t = 0; t < maxThreads; t++)
                threads[t].Join();
        }
    }
}
