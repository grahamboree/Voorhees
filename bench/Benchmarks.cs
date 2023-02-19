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

        Voorhees.JsonMapper voorhees_mapper = new Voorhees.JsonMapper();
        Voorhees.JsonTokenReader citm_tokenReader;
        Voorhees.JsonTokenReader canada_tokenReader;

        public JsonValueReadBench() {
            citm = File.ReadAllText("../../../../../../citm_catalog.json");
            canada = File.ReadAllText("../../../../../../canada.json");
            
        }

        [IterationSetup]
        public void Setup_Citm() {
            citm_reader = new StringReader(citm);
            canada_reader = new StringReader(canada);
        }

        [IterationSetup(Target = nameof(Read_Citm_Voorhees))]
        public void Setup_Voorhees_citm() {
            citm_tokenReader = new Voorhees.JsonTokenReader(new StringReader(citm));
        }

        [IterationSetup(Target = nameof(Read_Canada_Voorhees))]
        public void Setup_Voorhees_Canada() {
            canada_tokenReader = new Voorhees.JsonTokenReader(new StringReader(canada));
        }

        [Benchmark]
        public Voorhees.JsonValue Read_Citm_Voorhees() => voorhees_mapper.Read<Voorhees.JsonValue>(citm_tokenReader);

        [Benchmark]
        public Voorhees.JsonValue Read_Canada_Voorhees() => voorhees_mapper.Read<Voorhees.JsonValue>(canada_tokenReader);

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
        const int ITERATIONS = 100;

        string citm;
        string canada;

        public JsonValueWriteBench() {
            citm = File.ReadAllText("../../../../../../citm_catalog.json");
            canada = File.ReadAllText("../../../../../../canada.json");
        }

        #region Voorhees
        Voorhees.JsonMapper voorhees_mapper = new Voorhees.JsonMapper();
        Voorhees.JsonTokenWriter voorhees_tokenWriter = new Voorhees.JsonTokenWriter(new StreamWriter(Stream.Null), false);

        Voorhees.JsonValue voorhees_citm;

        [IterationSetup(Target = nameof(Write_Citm_Voorhees))]
        public void Setup_Citm_Voorhees() {
            voorhees_citm = Voorhees.JsonMapper.FromJson<Voorhees.JsonValue>(citm);
        }

        [Benchmark]
        public void Write_Citm_Voorhees() {
            voorhees_mapper.Write(voorhees_citm, voorhees_tokenWriter);
        }

        Voorhees.JsonValue voorhees_canada;

        [IterationSetup(Target = nameof(Write_Canada_Voorhees))]
        public void Setup_Canada_Voorhees() {
            voorhees_canada = Voorhees.JsonMapper.FromJson<Voorhees.JsonValue>(canada);
        }

        [Benchmark]
        public void Write_Canada_Voorhees() {
            voorhees_mapper.Write(voorhees_canada, voorhees_tokenWriter);
        }
        #endregion
        
        #region LitJson
        LitJson.JsonData litjson_citm;
        LitJson.JsonData litjson_canada;

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