using System.IO;
using BenchmarkDotNet.Attributes;

namespace bench {
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class JsonValueReadBench {
        string citm;
        string canada;

        public JsonValueReadBench() {
            citm = File.ReadAllText("../../../../../../citm_catalog.json");
            canada = File.ReadAllText("../../../../../../canada.json");
        }

        [Benchmark]
        public void Read_Citm_Voorhees() => Voorhees.JsonMapper.FromJson<Voorhees.JsonValue>(citm);

        [Benchmark]
        public void Read_Canada_Voorhees() => Voorhees.JsonMapper.FromJson<Voorhees.JsonValue>(canada);

        [Benchmark]
        public void Read_Citm_LitJson() => LitJson.JsonMapper.ToObject(new StringReader(citm));

        [Benchmark]
        public void Read_Canada_LitJson() => LitJson.JsonMapper.ToObject(new StringReader(canada));

        [Benchmark]
        public void Read_Citm_NewtonSoft() => Newtonsoft.Json.Linq.JObject.Load(new Newtonsoft.Json.JsonTextReader(new StringReader(citm)));

        [Benchmark]
        public void Read_Canada_NewtonSoft() => Newtonsoft.Json.Linq.JObject.Load(new Newtonsoft.Json.JsonTextReader(new StringReader(canada)));

        [Benchmark]
        public void Read_Citm_SystemTextJson() => System.Text.Json.Nodes.JsonNode.Parse(citm);
    }
    
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class JsonValueWriteBench {
        string citm;
        string canada;

        public JsonValueWriteBench() {
            citm = File.ReadAllText("../../../../../../citm_catalog.json");
            canada = File.ReadAllText("../../../../../../canada.json");

            voorhees_citm = Voorhees.JsonMapper.FromJson<Voorhees.JsonValue>(citm);
            voorhees_canada = Voorhees.JsonMapper.FromJson<Voorhees.JsonValue>(canada);

            litjson_citm = LitJson.JsonMapper.ToObject(citm);
            litjson_canada = LitJson.JsonMapper.ToObject(canada);

            newtonsoft_citm = Newtonsoft.Json.Linq.JObject.Load(new Newtonsoft.Json.JsonTextReader(new StringReader(citm)));
            newtonsoft_canada = Newtonsoft.Json.Linq.JObject.Load(new Newtonsoft.Json.JsonTextReader(new StringReader(canada)));

            stj_citm = System.Text.Json.Nodes.JsonNode.Parse(citm);
            stj_canada = System.Text.Json.Nodes.JsonNode.Parse(canada);
        }

        #region Voorhees
        Voorhees.JsonValue voorhees_citm;
        Voorhees.JsonValue voorhees_canada;

        [Benchmark]
        public void Write_Citm_Voorhees() {
            var voorhees_tokenWriter = new Voorhees.JsonTokenWriter(TextWriter.Null, false);
            var voorhees_mapper = new Voorhees.JsonMapper();
            voorhees_mapper.Write(voorhees_citm, voorhees_tokenWriter);
        }

        [Benchmark]
        public void Write_Canada_Voorhees() {
            var voorhees_tokenWriter = new Voorhees.JsonTokenWriter(TextWriter.Null, false);
            var voorhees_mapper = new Voorhees.JsonMapper();
            voorhees_mapper.Write(voorhees_canada, voorhees_tokenWriter);
        }
        #endregion
        
        #region LitJson
        LitJson.JsonData litjson_citm;
        LitJson.JsonData litjson_canada;
        
        [Benchmark]
        public void Write_Citm_LitJson() {
            var writer = new LitJson.JsonWriter(TextWriter.Null);
            litjson_citm.ToJson(writer);
        }
        
        [Benchmark]
        public void Write_Canada_LitJson() {
            var writer = new LitJson.JsonWriter(TextWriter.Null);
            litjson_canada.ToJson(writer);
        }
        #endregion
        
        #region Newtonsoft
        Newtonsoft.Json.Linq.JObject newtonsoft_citm;
        Newtonsoft.Json.Linq.JObject newtonsoft_canada;

        [Benchmark]
        public void Write_Citm_Newtonsoft() {
            var writer = new Newtonsoft.Json.JsonTextWriter(TextWriter.Null);
            newtonsoft_citm.WriteTo(writer);
        }
        
        [Benchmark]
        public void Write_Canada_Newtonsoft() {
            var writer = new Newtonsoft.Json.JsonTextWriter(TextWriter.Null);
            newtonsoft_canada.WriteTo(writer);
        }
        #endregion

        #region System.Text.Json
        System.Text.Json.Nodes.JsonNode stj_citm;
        System.Text.Json.Nodes.JsonNode stj_canada;

        [Benchmark]
        public void Write_Citm_SystemTextJson() {
            System.Text.Json.JsonSerializer.Serialize(Stream.Null, stj_citm);
        }

        [Benchmark]
        public void Write_Canada_SystemTextJson() {
            System.Text.Json.JsonSerializer.Serialize(Stream.Null, stj_canada);
        }
        #endregion
    }
}