using System;
using System.Linq;
using System.Text.Json;

using Barbados.Documents.Json;

namespace Barbados.Documents.Tests
{
	public sealed partial class BarbadosDocumentTest
	{
		private const string _field = "test";

		public sealed class HasValue
		{
			[Test]
			public void DocumentKeyExists_ReturnsFalse()
			{
				var nested = new BarbadosDocument.Builder()
					.Add(_field, "value")
					.Build();
				var document = new BarbadosDocument.Builder()
					.Add(_field, nested)
					.Build();

				var r = document.HasValue(_field);
				Assert.That(r, Is.False);
			}

			[Test]
			public void ValueKeyExist_ReturnsTrue()
			{
				var document = new BarbadosDocument.Builder()
					.Add(_field, "value")
					.Build();

				var r = document.HasValue(_field);
				Assert.That(r, Is.True);
			}

			[Test]
			public void ValueKeyDoesNotExist_ReturnsFalse()
			{
				var document = new BarbadosDocument.Builder()
					.Add(_field, "value")
					.Build();

				var r = document.HasValue("other-field");
				Assert.That(r, Is.False);
			}

			[Test]
			public void NestedValueKey_ReturnsTrue()
			{
				var document = new BarbadosDocument.Builder()
					.Add("doc.v1", 1)
					.Build();

				var r = document.HasValue("doc.v1");
				Assert.That(r, Is.True);
			}

			[Test]
			public void NestedDocumentKey_ReturnsFalse()
			{
				var document = new BarbadosDocument.Builder()
					.Add("doc.v1", 1)
					.Build();

				var r = document.HasValue("doc");
				Assert.That(r, Is.False);
			}
		}

		public sealed class HasDocument
		{
			[Test]
			public void ValueKeyExists_ReturnsFalse()
			{
				var document = new BarbadosDocument.Builder()
					.Add(_field, "value")
					.Build();

				var r = document.HasDocument(_field);
				Assert.That(r, Is.False);
			}

			[Test]
			public void DocumentKeyExist_ReturnsTrue()
			{
				var nested = new BarbadosDocument.Builder()
					.Add(_field, "value")
					.Build();
				var document = new BarbadosDocument.Builder()
					.Add(_field, nested)
					.Build();

				var r = document.HasDocument(_field);
				Assert.That(r, Is.True);
			}

			[Test]
			public void DocumentKeyDoesNotExist_ReturnsFalse()
			{
				var nested = new BarbadosDocument.Builder()
					.Add(_field, "value")
					.Build();
				var document = new BarbadosDocument.Builder()
					.Add(_field, nested)
					.Build();

				var r = document.HasDocument("other-field");
				Assert.That(r, Is.False);
			}

			[Test]
			public void NestedDocumentKey_ReturnsTrue()
			{
				var document = new BarbadosDocument.Builder()
					.Add("doc.v1", 1)
					.Build();

				var r = document.HasDocument("doc");
				Assert.That(r, Is.True);
			}

			[Test]
			public void NestedValueKey_ReturnsFalse()
			{
				var document = new BarbadosDocument.Builder()
					.Add("doc.v1", 1)
					.Build();

				var r = document.HasDocument("doc.v1");
				Assert.That(r, Is.False);
			}
		}

		public sealed class TryGet
		{
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
			public void Omni()
			{
				foreach (var (k, v) in _omniDocumentKV)
				{
					var r = _omniDocument.TryGet(k, out var got);
					Assert.Multiple(() =>
					{
						Assert.That(r, Is.True);
						Assert.That(got, Is.EqualTo(v));
					});
				}
			}

			[Test]
			public void ExplicitKeyNesting()
			{
				var document = new BarbadosDocument.Builder()
					.Add("doc.v1", 1)
					.Add("doc.v2", "str")
					.Add("a.b.c", true)
					.Build();

				using (Assert.EnterMultipleScope())
				{
					Assert.That(document.TryGetInt32("doc.v1", out var v1), Is.True);
					Assert.That(document.TryGetString("doc.v2", out var v2), Is.True);
					Assert.That(document.TryGetBoolean("a.b.c", out var v3), Is.True);
					Assert.That(v1, Is.EqualTo(1));
					Assert.That(v2, Is.EqualTo("str"));
					Assert.That(v3, Is.True);

					Assert.That(document.TryGetDocument("doc", out var doc), Is.True);
					Assert.That(doc.TryGetInt32("v1", out var dv1), Is.True);
					Assert.That(doc.TryGetString("v2", out var dv2), Is.True);
					Assert.That(dv1, Is.EqualTo(1));
					Assert.That(dv2, Is.EqualTo("str"));

					Assert.That(document.TryGetDocument("a", out var a), Is.True);
					Assert.That(a.TryGetBoolean("b.c", out var abc1), Is.True);
					Assert.That(abc1, Is.True);

					Assert.That(a.TryGetDocument("b", out var ab), Is.True);
					Assert.That(ab.TryGetBoolean("c", out var abc2), Is.True);
					Assert.That(abc2, Is.True);

					Assert.That(document.TryGetDocument("a.b", out var ab2), Is.True);
					Assert.That(ab2.TryGetBoolean("c", out var abc3), Is.True);
					Assert.That(abc3, Is.True);
				}
			}
		}

		public sealed class TryGetArrayLength
		{
			[Test]
			public void KeyExists_ValueIsNotArray_ReturnsFalse()
			{
				var document = new BarbadosDocument.Builder()
					.Add(_field, 1)
					.Build();

				var r = document.TryGetArrayLength(_field, out _);
				Assert.That(r, Is.False);
			}

			[Test]
			public void KeyDoesNotExist_ReturnsFalse()
			{
				var document = new BarbadosDocument.Builder()
					.Add(_field, [1, 2, 3])
					.Build();

				var r = document.TryGetArrayLength("other-field", out _);
				Assert.That(r, Is.False);
			}

			[Test]
			public void KeyExists_ReturnsCorrectLength()
			{
				var document = new BarbadosDocument.Builder()
					.Add(_field, [1, 2, 3])
					.Build();

				var r = document.TryGetArrayLength(_field, out var length);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(r, Is.True);
					Assert.That(length, Is.EqualTo(3));
				}
			}

			[Test]
			public void ViaExplicitNesting_KeyDoesNotExist_ReturnsFalse()
			{
				var document = new BarbadosDocument.Builder()
					.Add("doc.arr", [1, 2, 3])
					.Build();

				var r = document.TryGetArrayLength("doc.other", out _);
				Assert.That(r, Is.False);
			}

			[Test]
			public void ViaExplicitNesting_KeyExists_ReturnsCorrectLength()
			{
				var key = "doc.arr";
				var document = new BarbadosDocument.Builder()
					.Add(key, [1, 2, 3])
					.Build();

				var r = document.TryGetArrayLength(key, out var length);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(r, Is.True);
					Assert.That(length, Is.EqualTo(3));
				}
			}
		}

		public sealed class ToJson
		{
			[Test]
			public void EmptyDocument()
			{
				var document = BarbadosDocument.Empty;

				var json = document.ToJson();

				Assert.DoesNotThrow(() => JsonDocument.Parse(json));
				Assert.That(
					JsonDocument.Parse(json).RootElement.EnumerateObject().Count(), Is.Zero
				);
			}

			[Test]
			public void Omni()
			{
				// Non-ASCII keys get encoded as unicode escape symbols by default. We skip such keys
				// in here to not complicate the test further. Note that such values get deserialised
				// correctly by default, the only thing which needs extra care is the lookup part

				var json = _omniDocument.ToJson();
				Assert.DoesNotThrow(() => JsonDocument.Parse(json));
				var root = JsonDocument.Parse(json).RootElement;

				var d1 = root.GetProperty("d1");
				var d1d1 = d1.GetProperty("d1");
				var d1da = d1.GetProperty("da");
				var d2arr = root.GetProperty("d2").GetProperty("arr");

				Assert.Multiple(() =>
				{
					Assert.That(root.GetProperty("v-u8").GetByte(), Is.EqualTo((byte)_omniDocumentKV["v-u8"]));
					Assert.That(root.GetProperty("v-i8").GetSByte(), Is.EqualTo((sbyte)_omniDocumentKV["v-i8"]));
					Assert.That(root.GetProperty("v-u16").GetUInt16(), Is.EqualTo((ushort)_omniDocumentKV["v-u16"]));
					Assert.That(root.GetProperty("v-i16").GetInt16(), Is.EqualTo((short)_omniDocumentKV["v-i16"]));
					Assert.That(root.GetProperty("v-u32").GetUInt32(), Is.EqualTo((uint)_omniDocumentKV["v-u32"]));
					Assert.That(root.GetProperty("v-i32").GetInt32(), Is.EqualTo((int)_omniDocumentKV["v-i32"]));
					Assert.That(root.GetProperty("v-u64").GetUInt64(), Is.EqualTo((ulong)_omniDocumentKV["v-u64"]));
					Assert.That(root.GetProperty("v-i64").GetInt64(), Is.EqualTo((long)_omniDocumentKV["v-i64"]));
					Assert.That(root.GetProperty("v-f32").GetSingle(), Is.EqualTo((float)_omniDocumentKV["v-f32"]));
					Assert.That(root.GetProperty("v-f64").GetDouble(), Is.EqualTo((double)_omniDocumentKV["v-f64"]));
					Assert.That(root.GetProperty("v-dt").Deserialize<DateTime>(), Is.EqualTo((DateTime)_omniDocumentKV["v-dt"]));
					Assert.That(root.GetProperty("v-bool").GetBoolean(), Is.EqualTo((bool)_omniDocumentKV["v-bool"]));
					Assert.That(root.GetProperty("v-str").GetString(), Is.EqualTo((string)_omniDocumentKV["v-str"]));
					Assert.That(root.GetProperty("v-str-e").GetString(), Is.EqualTo((string)_omniDocumentKV["v-str-e"]));
					Assert.That(root.GetProperty("v-str-u").GetString(), Is.EqualTo((string)_omniDocumentKV["v-str-u"]));

					Assert.That(root.GetProperty("a-i8").Deserialize<sbyte[]>(), Is.EqualTo((sbyte[])_omniDocumentKV["a-i8"]));
					Assert.That(root.GetProperty("a-i16").Deserialize<short[]>(), Is.EqualTo((short[])_omniDocumentKV["a-i16"]));
					Assert.That(root.GetProperty("a-i32").Deserialize<int[]>(), Is.EqualTo((int[])_omniDocumentKV["a-i32"]));
					Assert.That(root.GetProperty("a-i64").Deserialize<long[]>(), Is.EqualTo((long[])_omniDocumentKV["a-i64"]));
					Assert.That(root.GetProperty("a-u8").Deserialize<byte[]>(), Is.EqualTo((byte[])_omniDocumentKV["a-u8"]));
					Assert.That(root.GetProperty("a-u16").Deserialize<ushort[]>(), Is.EqualTo((ushort[])_omniDocumentKV["a-u16"]));
					Assert.That(root.GetProperty("a-u32").Deserialize<uint[]>(), Is.EqualTo((uint[])_omniDocumentKV["a-u32"]));
					Assert.That(root.GetProperty("a-u64").Deserialize<ulong[]>(), Is.EqualTo((ulong[])_omniDocumentKV["a-u64"]));
					Assert.That(root.GetProperty("a-f32").Deserialize<float[]>(), Is.EqualTo((float[])_omniDocumentKV["a-f32"]));
					Assert.That(root.GetProperty("a-f64").Deserialize<double[]>(), Is.EqualTo((double[])_omniDocumentKV["a-f64"]));
					Assert.That(root.GetProperty("a-dt").Deserialize<DateTime[]>(), Is.EqualTo((DateTime[])_omniDocumentKV["a-dt"]));
					Assert.That(root.GetProperty("a-bool").Deserialize<bool[]>(), Is.EqualTo((bool[])_omniDocumentKV["a-bool"]));
					Assert.That(root.GetProperty("a-str").Deserialize<string[]>(), Is.EqualTo((string[])_omniDocumentKV["a-str"]));
					Assert.That(root.GetProperty("a-str-e").Deserialize<string[]>(), Is.EqualTo((string[])_omniDocumentKV["a-str-e"]));
					Assert.That(root.GetProperty("a-str-1e").Deserialize<string[]>(), Is.EqualTo((string[])_omniDocumentKV["a-str-1e"]));
					Assert.That(root.GetProperty("a-i32-e").Deserialize<int[]>(), Is.EqualTo((int[])_omniDocumentKV["a-i32-e"]));

					Assert.That(d1.GetProperty("v1").GetInt32(), Is.EqualTo((int)_omniDocumentKV["d1.v1"]));
					Assert.That(d1.GetProperty("v2").GetString(), Is.EqualTo((string)_omniDocumentKV["d1.v2"]));

					Assert.That(d1d1.GetProperty("v1").GetInt32(), Is.EqualTo((int)_omniDocumentKV["d1.d1.v1"]));
					Assert.That(d1d1.GetProperty("v2").GetString(), Is.EqualTo((string)_omniDocumentKV["d1.d1.v2"]));

					Assert.That(d1da.GetProperty("0").GetProperty("dav").GetString(), Is.EqualTo((string)_omniDocumentKV["d1.da.0.dav"]));
					Assert.That(d1da.GetProperty("1").GetProperty("dav").GetString(), Is.EqualTo((string)_omniDocumentKV["d1.da.1.dav"]));

					Assert.That(d2arr.GetProperty("0").GetProperty("i").GetInt32(), Is.EqualTo((int)_omniDocumentKV["d2.arr.0.i"]));
					Assert.That(d2arr.GetProperty("0").GetProperty("s").GetString(), Is.EqualTo((string)_omniDocumentKV["d2.arr.0.s"]));
					Assert.That(d2arr.GetProperty("1").GetProperty("i").GetInt32(), Is.EqualTo((int)_omniDocumentKV["d2.arr.1.i"]));
					Assert.That(d2arr.GetProperty("1").GetProperty("s").GetString(), Is.EqualTo((string)_omniDocumentKV["d2.arr.1.s"]));
					Assert.That(d2arr.GetProperty("2").GetProperty("i").GetInt32(), Is.EqualTo((int)_omniDocumentKV["d2.arr.2.i"]));
					Assert.That(d2arr.GetProperty("2").GetProperty("s").GetString(), Is.EqualTo((string)_omniDocumentKV["d2.arr.2.s"]));
				});
			}
		}
	}
}
