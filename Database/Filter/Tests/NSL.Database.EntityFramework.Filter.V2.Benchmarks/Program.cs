using BenchmarkDotNet.Running;
using NSL.Database.EntityFramework.Filter.V2.Benchmarks;

// Запускаем бенчмарки
BenchmarkRunner.Run<OperatorBenchmark>();