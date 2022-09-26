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
        public Voorhees.JsonValue Read_Citm_Voorhees() => Voorhees.JsonMapper.FromJson(citm);

        [Benchmark]
        public Voorhees.JsonValue Read_Canada_Voorhees() => Voorhees.JsonMapper.FromJson(canada);

        [Benchmark]
        public LitJson.JsonData Read_Citm_LitJson() => LitJson.JsonMapper.ToObject(citm);

        [Benchmark]
        public LitJson.JsonData Read_Canada_LitJson() => LitJson.JsonMapper.ToObject(canada);

        [Benchmark]
        public Newtonsoft.Json.Linq.JObject Read_Citm_NewtonSoft() {
            using var sr = new StringReader(citm);
            var reader = new Newtonsoft.Json.JsonTextReader(sr);
            return Newtonsoft.Json.Linq.JObject.Load(reader);
        }

        [Benchmark]
        public Newtonsoft.Json.Linq.JObject Read_Canada_NewtonSoft() {
            using var sr = new StringReader(canada);
            var reader = new Newtonsoft.Json.JsonTextReader(sr);
            return Newtonsoft.Json.Linq.JObject.Load(reader);
        }
    }
    
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class JsonValueWriteBench {
        string citm;
        string canada;
        
        Voorhees.JsonValue voorhees_canada;
        Voorhees.JsonValue voorhees_citm;

        public JsonValueWriteBench() {
            citm = File.ReadAllText("../../../../../../citm_catalog.json");
            canada = File.ReadAllText("../../../../../../canada.json");
            
            voorhees_citm = Voorhees.JsonMapper.FromJson(citm);
            voorhees_canada = Voorhees.JsonMapper.FromJson(canada);
        }

        #region Voorhees citm
        [Benchmark]
        public string Write_Citm_Voorhees() => Voorhees.JsonMapper.ToJson(voorhees_citm);
        #endregion
        
        #region Voorhees canada
        [Benchmark]
        public string Write_Canada_Voorhees() => Voorhees.JsonMapper.ToJson(voorhees_canada);
        #endregion
        
        #region LitJson citm
        LitJson.JsonData litjson_citm;
        
        [IterationSetup(Target = nameof(Write_Citm_LitJson))]
        public void Setup_Citm_LitJson() {
            litjson_citm = LitJson.JsonMapper.ToObject(citm);
        }
        
        [Benchmark]
        public string Write_Citm_LitJson() {
            TextWriter textWriter = new StringWriter();
            var writer = new LitJson.JsonWriter(textWriter);
            litjson_citm.ToJson(writer);
            return textWriter.ToString();
        }
        #endregion

        #region LitJson canada
        LitJson.JsonData litjson_canada;
        
        [IterationSetup(Target = nameof(Write_Canada_LitJson))]
        public void Setup_Canada_LitJson() {
            litjson_canada = LitJson.JsonMapper.ToObject(canada);
        }
        
        [Benchmark]
        public string Write_Canada_LitJson() {
            TextWriter textWriter = new StringWriter();
            var writer = new LitJson.JsonWriter(textWriter);
            litjson_canada.ToJson(writer);
            return textWriter.ToString();
        }
        #endregion
        
        #region Newtonsoft citm
        Newtonsoft.Json.Linq.JObject newtonsoft_citm;
        
        [IterationSetup(Target = nameof(Write_Citm_Newtonsoft))]
        public void Setup_Citm_Newtonsoft() {
            newtonsoft_citm = Newtonsoft.Json.Linq.JObject.Load(new Newtonsoft.Json.JsonTextReader(new StringReader(citm)));
        }

        [Benchmark]
        public string Write_Citm_Newtonsoft() {
            TextWriter textWriter = new StringWriter();
            var writer = new Newtonsoft.Json.JsonTextWriter(textWriter);
            newtonsoft_citm.WriteTo(writer);
            return textWriter.ToString();
        }
        #endregion

        #region Newtonsoft canada
        Newtonsoft.Json.Linq.JObject newtonsoft_canada;
        
        [IterationSetup(Target = nameof(Write_Canada_Newtonsoft))]
        public void Setup_Canada_Newtonsoft() {
            newtonsoft_canada = Newtonsoft.Json.Linq.JObject.Load(new Newtonsoft.Json.JsonTextReader(new StringReader(canada)));
        }
        
        [Benchmark]
        public string Write_Canada_Newtonsoft() {
            TextWriter textWriter = new StringWriter();
            var writer = new Newtonsoft.Json.JsonTextWriter(textWriter);
            newtonsoft_canada.WriteTo(writer);
            return textWriter.ToString();
        }
        #endregion
    }
}