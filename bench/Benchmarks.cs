using System.IO;
using BenchmarkDotNet.Attributes;

namespace bench {
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class JsonValueReadBench {
        string citm;
        string canada;

        StringReader citm_reader;
        StringReader canada_reader;

        public JsonValueReadBench() {
            citm = File.ReadAllText("../../../../../../citm_catalog.json");
            canada = File.ReadAllText("../../../../../../canada.json");
            
        }

        [IterationSetup]
        public void Setup_Citm() {
            citm_reader = new StringReader(citm);
            canada_reader = new StringReader(canada);
        }

        [Benchmark]
        public Voorhees.JsonValue Read_Citm_Voorhees() => Voorhees.JsonMapper.FromJson(citm_reader);

        [Benchmark]
        public Voorhees.JsonValue Read_Canada_Voorhees() => Voorhees.JsonMapper.FromJson(canada_reader);

        [Benchmark]
        public LitJson.JsonData Read_Citm_LitJson() => LitJson.JsonMapper.ToObject(citm_reader);

        [Benchmark]
        public LitJson.JsonData Read_Canada_LitJson() => LitJson.JsonMapper.ToObject(canada_reader);

        [Benchmark]
        public Newtonsoft.Json.Linq.JObject Read_Citm_NewtonSoft() {
            var reader = new Newtonsoft.Json.JsonTextReader(citm_reader);
            return Newtonsoft.Json.Linq.JObject.Load(reader);
        }

        [Benchmark]
        public Newtonsoft.Json.Linq.JObject Read_Canada_NewtonSoft() {
            var reader = new Newtonsoft.Json.JsonTextReader(canada_reader);
            return Newtonsoft.Json.Linq.JObject.Load(reader);
        }
    }
    
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class JsonValueWriteBench {
        Voorhees.JsonValue voorhees_canada;
        Voorhees.JsonValue voorhees_citm;

        [IterationSetup(Target = nameof(Write_Citm_Voorhees))]
        public void Setup_Citm_Voorhees() {
            voorhees_citm = Voorhees.JsonMapper.FromJson(new StreamReader(File.OpenRead("../../../../../../citm_catalog.json")));
        }

        [Benchmark]
        public string Write_Citm_Voorhees() => Voorhees.JsonMapper.ToJson(voorhees_citm);

        [IterationSetup(Target = nameof(Write_Canada_Voorhees))]
        public void Setup_Canada_Voorhees() {
            voorhees_canada = Voorhees.JsonMapper.FromJson(new StreamReader(File.OpenRead("../../../../../../canada.json")));
        }

        [Benchmark]
        public string Write_Canada_Voorhees() => Voorhees.JsonMapper.ToJson(voorhees_canada);
        
        #region LitJson
        LitJson.JsonData litjson_citm;
        LitJson.JsonData litjson_canada;

        [IterationSetup(Target = nameof(Write_Citm_LitJson))]
        public void Setup_Citm_LitJson() {
            var citm = File.ReadAllText("../../../../../../citm_catalog.json");
            litjson_citm = LitJson.JsonMapper.ToObject(citm);
        }
        
        [Benchmark]
        public string Write_Citm_LitJson() {
            TextWriter textWriter = new StringWriter();
            var writer = new LitJson.JsonWriter(textWriter);
            litjson_citm.ToJson(writer);
            return textWriter.ToString();
        }
        
        [IterationSetup(Target = nameof(Write_Canada_LitJson))]
        public void Setup_Canada_LitJson() {
            var canada = File.ReadAllText("../../../../../../canada.json");
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
        Newtonsoft.Json.Linq.JObject newtonsoft_canada;
        
        [IterationSetup(Target = nameof(Write_Citm_Newtonsoft))]
        public void Setup_Citm_Newtonsoft() {
            var citm = File.ReadAllText("../../../../../../citm_catalog.json");
            newtonsoft_citm = Newtonsoft.Json.Linq.JObject.Load(new Newtonsoft.Json.JsonTextReader(new StringReader(citm)));
        }

        [Benchmark]
        public string Write_Citm_Newtonsoft() {
            TextWriter textWriter = new StringWriter();
            var writer = new Newtonsoft.Json.JsonTextWriter(textWriter);
            newtonsoft_citm.WriteTo(writer);
            return textWriter.ToString();
        }
        
        [IterationSetup(Target = nameof(Write_Canada_Newtonsoft))]
        public void Setup_Canada_Newtonsoft() {
            var canada = File.ReadAllText("../../../../../../canada.json");
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