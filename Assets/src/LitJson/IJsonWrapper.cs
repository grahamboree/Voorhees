/**
 * IJsonWrapper.cs
 *   Interface that represents a type capable of handling all kinds of JSON
 *   data. This is mainly used when mapping objects through JsonMapper, and
 *   it's implemented by JsonData.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

using System.Collections.Generic;
using System.Collections.Specialized;

namespace LitJson {
	public enum JsonType {
		None,

		Object,
		Array,

		String,
		Boolean,
		Int,
		Float,
	}

	public interface IJsonWrapper : IList<JsonData>/*, IDictionary<string, JsonData>*/ {
		bool IsArray { get; }
		bool IsBoolean { get; }
		bool IsFloat { get; }
		bool IsInt { get; }
		bool IsObject{ get; }
		bool IsString { get; }

		JsonType Type { get; set; }
		bool BooleanValue { get; set; }
		float FloatValue { get; set; }
		int IntValue { get; set; }
		string StringValue { get; set; }



		string ToJson();
		void ToJson(JsonWriter writer);
	}
}
