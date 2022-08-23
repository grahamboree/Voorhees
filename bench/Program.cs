using BenchmarkDotNet.Running;

namespace bench {
    public class Program {
        public static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<JsonReaderBench>();
        }
    }
}