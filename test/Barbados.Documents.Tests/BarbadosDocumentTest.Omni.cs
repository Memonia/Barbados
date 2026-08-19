using System;
using System.Collections.Generic;

using Barbados.Documents.RadixTree;

namespace Barbados.Documents.Tests
{
	public partial class BarbadosDocumentTest
	{
		private static readonly string _longKeyValue = new('a', RadixTreeBuffer.MaxNodePrefixLength * 2);
		private static readonly string _longKeyDocument = new('b', RadixTreeBuffer.MaxNodePrefixLength * 2);

		private static readonly Dictionary<string, object> _omniDocumentKV = new() {
			{     "v-u8", (byte)8 },
			{     "v-i8", (sbyte)-8 },
			{    "v-u16", (ushort)16 },
			{    "v-i16", (short)-16 },
			{    "v-u32", (uint)32 },
			{    "v-i32", (int)-32 },
			{    "v-u64", (ulong)64 },
			{    "v-i64", (long)-64 },
			{    "v-f32", (float)32.32f },
			{    "v-f64", (double)32.32 },
			{     "v-dt", DateTime.UnixEpoch.AddYears(8) },
			{   "v-bool", true },
			{    "v-str", "str" },
			{  "v-str-u", "Μπαρμπάντος" },
			{     "a-i8", new sbyte[] { -8, 16, -32 } },
			{    "a-i16", new short[] { -16, 32, -64 } },
			{    "a-i32", new int[] { -32, 64, -128 } },
			{    "a-i64", new long[] { -64, 128, -256 } },
			{     "a-u8", new byte[] { 8, 16, 32 } },
			{    "a-u16", new ushort[] { 16, 32, 64 } },
			{    "a-u32", new uint[] { 32, 64, 128 } },
			{    "a-u64", new ulong[] { 64, 128, 256 } },
			{    "a-f32", new float[] { 32.32f, -64.64f, 128.128f } },
			{    "a-f64", new double[] { 64.64, -128.128, 256.256 } },
			{     "a-dt", new DateTime[] { DateTime.UnixEpoch.AddYears(8), DateTime.UnixEpoch.AddYears(16), DateTime.UnixEpoch.AddYears(32) } },
			{   "a-bool", new bool[] { true, false, true } },
			{    "a-str", new string[] { "Μπαρμπάντος", "巴巴多斯", "str3" } },
			{  "v-str-e", "" },
			{  "a-str-e", new string[] { "", "", "" } },
			{ "a-str-1e", new string[] { "str", "", "Μπαρμπάντος" } },
			{  "a-i32-e", Array.Empty<int>() },

			{   "d1.v1",          11 },
			{   "d1.v2",        "12" },
			{   "d1.d1.v1",      111 },
			{   "d1.d1.v2",    "112" },
			{   "d1.d2.v1",      121 },
			{   "d1.d2.v2",    "122" },
			{   "d1.d2.v3",     123f },
			{   "d1.da.0.dav", "da0" },
			{   "d1.da.1.dav", "da1" },

			{   "d2.arr.0.i",    0 },
			{   "d2.arr.0.s", "s0" },
			{   "d2.arr.1.i",    1 },
			{   "d2.arr.1.s", "s1" },
			{   "d2.arr.2.i",    2 },
			{   "d2.arr.2.s",   "" },

			{  "巴巴多斯.str.Μπαρμπάντος", "unicode" },

			{ _longKeyValue, "long-value" },
			{ $"{_longKeyDocument}.v1", 1 },
			{ $"{_longKeyDocument}.v2", "2" },
		};

		private static readonly BarbadosDocument _omniDocument = new BarbadosDocument.Builder()
			.Add("v-u8", _omniDocumentKV["v-u8"])
			.Add("v-i8", _omniDocumentKV["v-i8"])
			.Add("v-u16", _omniDocumentKV["v-u16"])
			.Add("v-i16", _omniDocumentKV["v-i16"])
			.Add("v-u32", _omniDocumentKV["v-u32"])
			.Add("v-i32", _omniDocumentKV["v-i32"])
			.Add("v-u64", _omniDocumentKV["v-u64"])
			.Add("v-i64", _omniDocumentKV["v-i64"])
			.Add("v-f32", _omniDocumentKV["v-f32"])
			.Add("v-f64", _omniDocumentKV["v-f64"])
			.Add("v-dt", _omniDocumentKV["v-dt"])
			.Add("v-bool", _omniDocumentKV["v-bool"])
			.Add("v-str", _omniDocumentKV["v-str"])
			.Add("a-i8", _omniDocumentKV["a-i8"])
			.Add("a-i16", _omniDocumentKV["a-i16"])
			.Add("a-i32", _omniDocumentKV["a-i32"])
			.Add("a-i64", _omniDocumentKV["a-i64"])
			.Add("a-u8", _omniDocumentKV["a-u8"])
			.Add("a-u16", _omniDocumentKV["a-u16"])
			.Add("a-u32", _omniDocumentKV["a-u32"])
			.Add("a-u64", _omniDocumentKV["a-u64"])
			.Add("a-f32", _omniDocumentKV["a-f32"])
			.Add("a-f64", _omniDocumentKV["a-f64"])
			.Add("a-dt", _omniDocumentKV["a-dt"])
			.Add("a-bool", _omniDocumentKV["a-bool"])
			.Add("a-str", _omniDocumentKV["a-str"])
			.Add("v-str-e", _omniDocumentKV["v-str-e"])
			.Add("v-str-u", _omniDocumentKV["v-str-u"])
			.Add("a-str-e", _omniDocumentKV["a-str-e"])
			.Add("a-str-1e", _omniDocumentKV["a-str-1e"])
			.Add("a-i32-e", _omniDocumentKV["a-i32-e"])
			.Add("巴巴多斯.str.Μπαρμπάντος", _omniDocumentKV["巴巴多斯.str.Μπαρμπάντος"])
			.Add("d1", new BarbadosDocument.Builder()
				.Add("v1", _omniDocumentKV["d1.v1"])
				.Add("v2", _omniDocumentKV["d1.v2"])
				.Add("d1", new BarbadosDocument.Builder()
					.Add("v1", _omniDocumentKV["d1.d1.v1"])
					.Add("v2", _omniDocumentKV["d1.d1.v2"])
					.Build()
				)
				.Add("d2", new BarbadosDocument.Builder()
					.Add("v1", _omniDocumentKV["d1.d2.v1"])
					.Add("v2", _omniDocumentKV["d1.d2.v2"])
					.Add("v3", _omniDocumentKV["d1.d2.v3"])
					.Build()
				)
				.Add("da", [
					new BarbadosDocument.Builder().Add("dav", _omniDocumentKV["d1.da.0.dav"]).Build(),
					new BarbadosDocument.Builder().Add("dav", _omniDocumentKV["d1.da.1.dav"]).Build(),
				])
				.Build()
			)
			.Add(_longKeyValue, _omniDocumentKV[_longKeyValue])
			.Add(_longKeyDocument, new BarbadosDocument.Builder()
				.Add("v1", _omniDocumentKV[$"{_longKeyDocument}.v1"])
				.Add("v2", _omniDocumentKV[$"{_longKeyDocument}.v2"])
				.Build()
			)
			.Add("d2", new BarbadosDocument.Builder()
				.Add("arr", [
					new BarbadosDocument.Builder().Add("i", _omniDocumentKV["d2.arr.0.i"]).Add("s", _omniDocumentKV["d2.arr.0.s"]).Build(),
					new BarbadosDocument.Builder().Add("i", _omniDocumentKV["d2.arr.1.i"]).Add("s", _omniDocumentKV["d2.arr.1.s"]).Build(),
					new BarbadosDocument.Builder().Add("i", _omniDocumentKV["d2.arr.2.i"]).Add("s", _omniDocumentKV["d2.arr.2.s"]).Build(),
				])
				.Build()
			)
			.Build()
		;
	}
}
