using System;
using System.Collections.Generic;

namespace Voorhees {
    public partial class JsonMapper {
        static readonly JsonMapper defaultInstance = new();

        /////////////////////////////////////////////////

        delegate void ExporterFunc(object obj, JsonTokenWriter os);
        delegate object ImporterFunc(JsonTokenReader tokenReader);

        readonly Dictionary<Type, ImporterFunc> importers = new();
        readonly Dictionary<Type, ExporterFunc> exporters = new();
    }
}
