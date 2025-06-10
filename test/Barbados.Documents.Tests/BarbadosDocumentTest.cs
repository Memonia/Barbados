using System;
using System.Linq;

namespace Barbados.Documents.Tests
{
	public sealed partial class BarbadosDocumentTest
	{
		public sealed class TryGet
		{
			private const string _field = "test";

			[Test]
			public void Int8()
			{
				var value = (sbyte)-8;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetInt8(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Int16()
			{
				var value = (short)-16;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetInt16(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Int32()
			{
				var value = -32;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetInt32(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Int64()
			{
				var value = -64L;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetInt64(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void UInt8()
			{
				var value = (byte)8;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetUInt8(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void UInt16()
			{
				var value = (ushort)16;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetUInt16(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void UInt32()
			{
				var value = (uint)32;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetUInt32(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void UInt64()
			{
				var value = (ulong)64;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetUInt64(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Float32()
			{
				var value = 32.32f;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetFloat32(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Float64()
			{
				var value = 64.64;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetFloat64(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void DateTime()
			{
				var value = System.DateTime.UnixEpoch.AddYears(8);
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetDateTime(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Boolean()
			{
				var value = true;
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetBoolean(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void String()
			{
				var value = "str";
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
					.Build();

				var r = document.TryGetString(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Int8Array()
			{
				var value = new sbyte[] { -8, -16, -32 };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetInt8Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Int16Array()
			{
				var value = new short[] { -16, -32, -64 };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetInt16Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Int32Array()
			{
				var value = new int[] { -32, -64, -128 };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetInt32Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Int64Array()
			{
				var value = new long[] { -64, -128, -256 };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetInt64Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void UInt8Array()
			{
				var value = new byte[] { 8, 16, 32 };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetUInt8Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void UInt16Array()
			{
				var value = new ushort[] { 16, 32, 64 };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetUInt16Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void UInt32Array()
			{
				var value = new uint[] { 32, 64, 128 };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetUInt32Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void UInt64Array()
			{
				var value = new ulong[] { 64, 128, 256 };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetUInt64Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Float32Array()
			{
				var value = new float[] { 32.32f, 64.64f, 128.128f };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetFloat32Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Float64Array()
			{
				var value = new double[] { 64.64, 128.128, 256.256 };
				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetFloat64Array(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void DateTimeArray()
			{
				var value = new DateTime[]
				{
					System.DateTime.UnixEpoch.AddYears(8),
					System.DateTime.UnixEpoch.AddYears(16),
					System.DateTime.UnixEpoch.AddYears(32)
				};

				var document = new BarbadosDocument.Builder()
				.Add(_field, value)
				.Build();

				var r = document.TryGetDateTimeArray(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void BooleanArray()
			{
				var value = new bool[] { true, false, true };
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
				.Build();

				var r = document.TryGetBooleanArray(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void StringArray_AnyValue()
			{
				var value = new string[] { "str1", "str2", "str3" };
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
				.Build();

				var r = document.TryGetStringArray(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void StringArray_NoValues()
			{
				var value = Array.Empty<string>();
				var document = new BarbadosDocument.Builder()
					.Add(_field, value)
				.Build();

				var r = document.TryGetStringArray(_field, out var got);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(got, Is.EqualTo(value));
				});
			}

			[Test]
			public void Document()
			{
				var inner = "inside";
				var outer = "document";
				var innerValue = "str";
				var innerDocument = new BarbadosDocument.Builder()
					.Add(inner, innerValue)
					.Build();

				var document = new BarbadosDocument.Builder()
					.Add(outer, innerDocument)
					.Build();

				var r1 = document.TryGetDocument(outer, out var extractedDocument);
				var r2 = extractedDocument.TryGetString(inner, out var extractedValue);

				Assert.Multiple(() =>
				{
					Assert.That(r1, Is.True);
					Assert.That(r2, Is.True);
					Assert.That(extractedValue, Is.EqualTo(innerValue));
				});
			}

			[Test]
			public void DocumentArray()
			{
				var inner = "inside";
				var outer = "documents";
				var innerValue1 = "str1";
				var innerValue2 = "str2";
				var innerDocument1 = new BarbadosDocument.Builder()
					.Add(inner, innerValue1)
					.Build();

				var innerDocument2 = new BarbadosDocument.Builder()
					.Add(inner, innerValue2)
					.Build();

				var document = new BarbadosDocument.Builder()
					.Add(outer, [innerDocument1, innerDocument2])
					.Build();

				var r = document.TryGetDocumentArray(outer, out var extracted);

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(extracted, Has.Length.EqualTo(2));
					Assert.That(extracted[0].TryGetString(inner, out var extractedValue1), Is.True);
					Assert.That(extracted[1].TryGetString(inner, out var extractedValue2), Is.True);
					Assert.That(extractedValue1, Is.EqualTo(innerValue1));
					Assert.That(extractedValue2, Is.EqualTo(innerValue2));
				});
			}

			[Test]
			public void EveryTypeFromOneDocument()
			{
				var builder = new BarbadosDocument.Builder();
				var (i8, i8v) = ("int8", (sbyte)-8);
				var (i16, i16v) = ("int16", (short)-16);
				var (i32, i32v) = ("int32", (int)-32);
				var (i64, i64v) = ("int64", (long)-64L);
				var (ui8, ui8v) = ("uint8", (byte)8);
				var (ui16, ui16v) = ("uint16", (ushort)16);
				var (ui32, ui32v) = ("uint32", (uint)32);
				var (ui64, ui64v) = ("uint64", (ulong)64);
				var (f32, f32v) = ("float32", 32.32f);
				var (f64, f64v) = ("float64", 64.64);
				var (dt, dtv) = ("datetime", System.DateTime.UnixEpoch.AddYears(8));
				var (b, bv) = ("bool", true);
				var (s, sv) = ("string", "str");
				var (i8a, i8av) = ("int8-array", new sbyte[] { -8, -16, -32 });
				var (i16a, i16av) = ("int16-array", new short[] { -16, -32, -64 });
				var (i32a, i32av) = ("int32-array", new int[] { -32, -64, -128 });
				var (i64a, i64av) = ("int64-array", new long[] { -64, -128, -256 });
				var (ui8a, ui8av) = ("uint8-array", new byte[] { 8, 16, 32 });
				var (ui16a, ui16av) = ("uint16-array", new ushort[] { 16, 32, 64 });
				var (ui32a, ui32av) = ("uint32-array", new uint[] { 32, 64, 128 });
				var (ui64a, ui64av) = ("uint64-array", new ulong[] { 64, 128, 256 });
				var (f32a, f32av) = ("float32-array", new float[] { 32.32f, 64.64f, 128.128f });
				var (f64a, f64av) = ("float64-array", new double[] { 64.64, 128.128, 256.256 });
				var (dta, dtav) = ("datetime-array", new DateTime[]
				{
					System.DateTime.UnixEpoch.AddYears(8),
					System.DateTime.UnixEpoch.AddYears(16),
					System.DateTime.UnixEpoch.AddYears(32)
				});
				var (ba, bav) = ("bool-array", new bool[] { true, false, true });
				var (sa, sav) = ("string-array", new string[] { "str1", "str2", "str3" });

				var nested = builder
					.Add(i8, i8v)
					.Add(i16, i16v)
					.Add(i32, i32v)
					.Add(i64, i64v)
					.Add(ui8, ui8v)
					.Add(ui16, ui16v)
					.Add(ui32, ui32v)
					.Add(ui64, ui64v)
					.Add(f32, f32v)
					.Add(f64, f64v)
					.Add(dt, dtv)
					.Add(b, bv)
					.Add(s, sv)
					.Add(i8a, i8av)
					.Add(i16a, i16av)
					.Add(i32a, i32av)
					.Add(i64a, i64av)
					.Add(ui8a, ui8av)
					.Add(ui16a, ui16av)
					.Add(ui32a, ui32av)
					.Add(ui64a, ui64av)
					.Add(f32a, f32av)
					.Add(f64a, f64av)
					.Add(dta, dtav)
					.Add(ba, bav)
					.Add(sa, sav)
					.Build();

				var d = "document";
				var da = "document-array";
				var document = builder
					.Add(d, nested)
					.Add(da, [nested, nested, nested])
					.Build();

				var r1 = document.TryGetDocument(d, out var extractedDocument);
				var r2 = document.TryGetDocumentArray(da, out var extractedDocumentArray);
				Assert.Multiple(() =>
				{
					Assert.That(r1, Is.True);
					Assert.That(r2, Is.True);
				});

				foreach (var doc in Enumerable.Concat([extractedDocument], extractedDocumentArray))
				{
					var f1 = doc.TryGetInt8(i8, out var i8ve);
					var f2 = doc.TryGetInt16(i16, out var i16ve);
					var f3 = doc.TryGetInt32(i32, out var i32ve);
					var f4 = doc.TryGetInt64(i64, out var i64ve);
					var f5 = doc.TryGetUInt8(ui8, out var ui8ve);
					var f6 = doc.TryGetUInt16(ui16, out var ui16ve);
					var f7 = doc.TryGetUInt32(ui32, out var ui32ve);
					var f8 = doc.TryGetUInt64(ui64, out var ui64ve);
					var f9 = doc.TryGetFloat32(f32, out var f32ve);
					var f10 = doc.TryGetFloat64(f64, out var f64ve);
					var f11 = doc.TryGetDateTime(dt, out var dte);
					var f12 = doc.TryGetBoolean(b, out var be);
					var f13 = doc.TryGetString(s, out var se);
					var f14 = doc.TryGetInt8Array(i8a, out var i8ave);
					var f15 = doc.TryGetInt16Array(i16a, out var i16ave);
					var f16 = doc.TryGetInt32Array(i32a, out var i32ave);
					var f17 = doc.TryGetInt64Array(i64a, out var i64ave);
					var f18 = doc.TryGetUInt8Array(ui8a, out var ui8ave);
					var f19 = doc.TryGetUInt16Array(ui16a, out var ui16ave);
					var f20 = doc.TryGetUInt32Array(ui32a, out var ui32ave);
					var f21 = doc.TryGetUInt64Array(ui64a, out var ui64ave);
					var f22 = doc.TryGetFloat32Array(f32a, out var f32ave);
					var f23 = doc.TryGetFloat64Array(f64a, out var f64ave);
					var f24 = doc.TryGetDateTimeArray(dta, out var dtave);
					var f25 = doc.TryGetBooleanArray(ba, out var bave);
					var f26 = doc.TryGetStringArray(sa, out var save);

					Assert.Multiple(() =>
					{
						Assert.That(f1, Is.True);
						Assert.That(f2, Is.True);
						Assert.That(f3, Is.True);
						Assert.That(f4, Is.True);
						Assert.That(f5, Is.True);
						Assert.That(f6, Is.True);
						Assert.That(f7, Is.True);
						Assert.That(f8, Is.True);
						Assert.That(f9, Is.True);
						Assert.That(f10, Is.True);
						Assert.That(f11, Is.True);
						Assert.That(f12, Is.True);
						Assert.That(f13, Is.True);
						Assert.That(f14, Is.True);
						Assert.That(f15, Is.True);
						Assert.That(f16, Is.True);
						Assert.That(f17, Is.True);
						Assert.That(f18, Is.True);
						Assert.That(f19, Is.True);
						Assert.That(f20, Is.True);
						Assert.That(f21, Is.True);
						Assert.That(f22, Is.True);
						Assert.That(f23, Is.True);
						Assert.That(f24, Is.True);
						Assert.That(f25, Is.True);
						Assert.That(f26, Is.True);

						Assert.That(i8v, Is.EqualTo(i8ve));
						Assert.That(i16v, Is.EqualTo(i16ve));
						Assert.That(i32v, Is.EqualTo(i32ve));
						Assert.That(i64v, Is.EqualTo(i64ve));
						Assert.That(ui8v, Is.EqualTo(ui8ve));
						Assert.That(ui16v, Is.EqualTo(ui16ve));
						Assert.That(ui32v, Is.EqualTo(ui32ve));
						Assert.That(ui64v, Is.EqualTo(ui64ve));
						Assert.That(f32v, Is.EqualTo(f32ve));
						Assert.That(f64v, Is.EqualTo(f64ve));
						Assert.That(dtv, Is.EqualTo(dte));
						Assert.That(bv, Is.EqualTo(be));
						Assert.That(sv, Is.EqualTo(se));
						Assert.That(i8av, Is.EqualTo(i8ave));
						Assert.That(i16av, Is.EqualTo(i16ave));
						Assert.That(i32av, Is.EqualTo(i32ave));
						Assert.That(i64av, Is.EqualTo(i64ave));
						Assert.That(ui8av, Is.EqualTo(ui8ave));
						Assert.That(ui16av, Is.EqualTo(ui16ave));
						Assert.That(ui32av, Is.EqualTo(ui32ave));
						Assert.That(ui64av, Is.EqualTo(ui64ave));
						Assert.That(f32av, Is.EqualTo(f32ave));
						Assert.That(f64av, Is.EqualTo(f64ave));
						Assert.That(dtav, Is.EqualTo(dtave));
						Assert.That(bav, Is.EqualTo(bave));
						Assert.That(sav, Is.EqualTo(save));
					});
				}
			}
		}
	}
}
