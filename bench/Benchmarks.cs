using System.IO;
using BenchmarkDotNet.Attributes;

namespace bench {
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class JsonReaderBench {
        string citm;
        string canada;
        
        public JsonReaderBench() {
            citm =   File.ReadAllText("../../../../../../citm_catalog.json");
            canada = File.ReadAllText("../../../../../../canada.json");
        }

        [Benchmark]
        public Voorhees.JsonValue Voorhees_Citm() => Voorhees.JsonMapper.FromJson(citm);

        [Benchmark]
        public Voorhees.JsonValue Voorhees_Canada() => Voorhees.JsonMapper.FromJson(canada);
        
        [Benchmark]
        public LitJson.JsonData LitJson_Citm() => LitJson.JsonMapper.ToObject(citm);

        [Benchmark]
        public LitJson.JsonData LitJson_Canada() => LitJson.JsonMapper.ToObject(canada);
        
        [Benchmark]
        public Newtonsoft.Json.Linq.JObject NewtonSoft_Citm() {
            using var sr = new StringReader(citm);
            var reader = new Newtonsoft.Json.JsonTextReader(sr);
            return Newtonsoft.Json.Linq.JObject.Load(reader);
        }

        [Benchmark]
        public Newtonsoft.Json.Linq.JObject NewtonSoft_Canada() {
            using var sr = new StringReader(canada);
            var reader = new Newtonsoft.Json.JsonTextReader(sr);
            return Newtonsoft.Json.Linq.JObject.Load(reader);
        }
    }
}