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
        public Voorhees.JsonValue Read_Citm_Voorhees() {
            var citm_tokenReader = new Voorhees.JsonTokenReader(new StringReader(citm));
            var voorhees_mapper = new Voorhees.JsonMapper();
            return voorhees_mapper.Read<Voorhees.JsonValue>(citm_tokenReader);
        }

        [Benchmark]
        public Voorhees.JsonValue Read_Canada_Voorhees() {
            var canada_tokenReader = new Voorhees.JsonTokenReader(new StringReader(canada));
            var voorhees_mapper = new Voorhees.JsonMapper();
            return voorhees_mapper.Read<Voorhees.JsonValue>(canada_tokenReader);
        }

        [Benchmark]
        public LitJson.JsonData Read_Citm_LitJson() => LitJson.JsonMapper.ToObject(new StringReader(citm));

        [Benchmark]
        public LitJson.JsonData Read_Canada_LitJson() => LitJson.JsonMapper.ToObject(new StringReader(canada));

        [Benchmark]
        public Newtonsoft.Json.Linq.JObject Read_Citm_NewtonSoft() {
            var reader = new Newtonsoft.Json.JsonTextReader(new StringReader(citm));
            return Newtonsoft.Json.Linq.JObject.Load(reader);
        }

        [Benchmark]
        public Newtonsoft.Json.Linq.JObject Read_Canada_NewtonSoft() {
            var reader = new Newtonsoft.Json.JsonTextReader(new StringReader(canada));
            return Newtonsoft.Json.Linq.JObject.Load(reader);
        }
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
        }

        #region Voorhees
        Voorhees.JsonValue voorhees_citm;
        Voorhees.JsonValue voorhees_canada;

        [Benchmark]
        public string Write_Citm_Voorhees() {
            TextWriter textWriter = new StringWriter();
            var voorhees_tokenWriter = new Voorhees.JsonTokenWriter(textWriter, false);
            var voorhees_mapper = new Voorhees.JsonMapper();
            voorhees_mapper.Write(voorhees_citm, voorhees_tokenWriter);
            return textWriter.ToString();
        }

        [Benchmark]
        public string Write_Canada_Voorhees() {
            TextWriter textWriter = new StringWriter();
            var voorhees_tokenWriter = new Voorhees.JsonTokenWriter(textWriter, false);
            var voorhees_mapper = new Voorhees.JsonMapper();
            voorhees_mapper.Write(voorhees_canada, voorhees_tokenWriter);
            return textWriter.ToString();
        }
        #endregion
        
        #region LitJson
        LitJson.JsonData litjson_citm;
        LitJson.JsonData litjson_canada;
        
        [Benchmark]
        public string Write_Citm_LitJson() {
            TextWriter textWriter = new StringWriter();
            var writer = new LitJson.JsonWriter(textWriter);
            litjson_citm.ToJson(writer);
            return textWriter.ToString();
        }
        
        [Benchmark]
        public string Write_Canada_LitJson() {
            TextWriter textWriter = new StringWriter();
            var writer = new LitJson.JsonWriter(textWriter);
            litjson_canada.ToJson(writer);
            return textWriter.ToString();
        }
        #endregion
        
        #region Newtonsoft
        Newtonsoft.Json.Linq.JObject newtonsoft_citm;
        Newtonsoft.Json.Linq.JObject newtonsoft_canada;

        [Benchmark]
        public string Write_Citm_Newtonsoft() {
            TextWriter textWriter = new StringWriter();
            var writer = new Newtonsoft.Json.JsonTextWriter(textWriter);
            newtonsoft_citm.WriteTo(writer);
            return textWriter.ToString();
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