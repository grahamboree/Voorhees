using BenchmarkDotNet.Running;

namespace bench {
    public class Program {
        public static void Main(string[] args) {
            var summary1 = BenchmarkRunner.Run<JsonValueReadBench>();
            var summary2 = BenchmarkRunner.Run<JsonValueWriteBench>();
        }
    }
}