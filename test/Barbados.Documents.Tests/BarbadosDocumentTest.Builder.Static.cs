namespace Barbados.Documents.Tests
{
	public partial class BarbadosDocumentTest
	{
		public partial class Builder
		{
			public sealed class Static
			{
				private static readonly BarbadosKey[] _valueKeys = ["v-u8", "v-i32", "v-bool", "v-str"];
				private static readonly BarbadosKey[] _arrayKeys = ["a-i8", "a-u32", "a-f64", "a-bool"];
				private static readonly BarbadosKey[] _stringArrayKeys = ["a-str", "a-str-e", "a-str-1e"];
				private static readonly BarbadosKey[] _documentKeys = ["d1", _longKeyDocument];
				private static readonly BarbadosKey[] _documentArrayKeys = ["d2"];
				private static readonly BarbadosKey[] _nestedDocumentKeys = ["d1.d1"];
				private static readonly BarbadosKey[] _nestedDocumentArrayKeys = ["d1.da"];
				private static readonly BarbadosKey[] _nestedMixedKeys = ["d1.d1", "d1.da", "d2.arr"];
				private static readonly BarbadosKey[] _valueAndDocumentKeys = ["v-u8", "v-str", "d1", "d2", _longKeyValue];
				private static readonly BarbadosKey[] _valueAndArrayKeys = ["v-i16", "v-u64", "a-i32", "a-str", _longKeyValue];

				private static readonly BarbadosKey[][] _keySets =
				[
					_valueKeys,
					_arrayKeys,
					_stringArrayKeys,
					_documentKeys,
					_documentArrayKeys,
					_nestedDocumentKeys,
					_nestedDocumentArrayKeys,
					_nestedMixedKeys,
					_valueAndDocumentKeys,
					_valueAndArrayKeys,
				];

				public sealed class FromBytes
				{
					[Test]
					public void OmniDocument()
					{
						var restored = BarbadosDocument.Builder.FromBytes(_omniDocument.AsBytes());
						Assert.That(_omniDocument, Is.EqualTo(restored));
					}
				}

				public sealed class FromBytesInclude
				{
					[Test]
					public void NonExistentKeys_ReturnsEmptyDocument()
					{
						var k = new BarbadosKey("does-not-exist");
						var result = BarbadosDocument.Builder.FromBytesInclude(_omniDocument.AsBytes(), [k]);

						Assert.That(result.Count(), Is.Zero);
					}

					[Test]
					public void FromEmptyDocument_ReturnsEmptyDocument()
					{
						var k = new BarbadosKey("k");
						var result = BarbadosDocument.Builder.FromBytesInclude(BarbadosDocument.Empty.AsBytes(), [k]);

						Assert.That(result.Count(), Is.Zero);
					}

					[TestCaseSource(typeof(Static), nameof(_keySets))]
					public void IncludedKeysExistWithCorrectValues(BarbadosKey[] keys)
					{
						var result = BarbadosDocument.Builder.FromBytesInclude(_omniDocument.AsBytes(), keys);
						foreach (var key in keys)
						{
							Assert.That(result.Exists(key), Is.True);
							if (BarbadosDocument.TryCompare(key, result, _omniDocument, out var cresult))
							{
								Assert.That(cresult, Is.Zero);
							}

							else
							{
								var rd = result.GetDocument(key);
								var od = _omniDocument.GetDocument(key);
								Assert.That(rd, Is.EqualTo(od));
							}
						}
					}
				}

				public sealed class FromBytesExclude
				{
					[Test]
					public void NonExistentKeys_ReturnsOriginalDocument()
					{
						var k = new BarbadosKey("does-not-exist");
						var result = BarbadosDocument.Builder.FromBytesExclude(_omniDocument.AsBytes(), [k]);

						Assert.That(result, Is.EqualTo(_omniDocument));
					}

					[Test]
					public void FromEmptyDocument_ReturnsEmptyDocument()
					{
						var k = new BarbadosKey("k");
						var result = BarbadosDocument.Builder.FromBytesExclude(BarbadosDocument.Empty.AsBytes(), [k]);

						Assert.That(result.Count(), Is.Zero);
					}

					[TestCaseSource(typeof(Static), nameof(_keySets))]
					public void ExcludedKeysDoNotExist(BarbadosKey[] keys)
					{
						var result = BarbadosDocument.Builder.FromBytesExclude(_omniDocument.AsBytes(), keys);
						foreach (var key in keys)
						{
							Assert.That(result.Exists(key), Is.False);
						}
					}
				}
			}
		}
	}
}
