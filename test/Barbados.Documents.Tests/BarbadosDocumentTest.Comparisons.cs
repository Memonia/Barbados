using System.Diagnostics.CodeAnalysis;

namespace Barbados.Documents.Tests
{
	public partial class BarbadosDocumentTest
	{
		public sealed class Static
		{
			public sealed class TryCompare
			{

				[Test]
				public void EmptyDocuments_ReturnsFalse()
				{
					var success = BarbadosDocument.TryCompare("key", BarbadosDocument.Empty, BarbadosDocument.Empty, out _);
					Assert.That(success, Is.False);
				}

				[Test]
				public void KeyDoesNotExistInOne_ReturnsFalse()
				{
					var a = new BarbadosDocument.Builder()
						.Add(_field, 1)
						.Build();

					var success = BarbadosDocument.TryCompare(_field, a, BarbadosDocument.Empty, out _);
					Assert.That(success, Is.False);
				}

				[Test]
				public void KeyDoesNotExistInBoth_ReturnsFalse()
				{
					var a = new BarbadosDocument.Builder()
						.Add(_field, 1)
						.Build();

					var b = new BarbadosDocument.Builder()
						.Add(_field, 2)
						.Build();

					var success = BarbadosDocument.TryCompare("does-not-exist", a, b, out _);
					Assert.That(success, Is.False);
				}

				[Test]
				public void OneKeyIsDocument_ReturnsFalse()
				{
					var a = new BarbadosDocument.Builder()
						.Add(_field, new BarbadosDocument.Builder()
							.Add("v", 1)
							.Build()
						)
						.Build();

					var b = new BarbadosDocument.Builder()
						.Add(_field, new BarbadosDocument.Builder()
							.Add("v", 1)
							.Build()
						)
						.Build();

					var success = BarbadosDocument.TryCompare(_field, a, b, out _);
					Assert.That(success, Is.False);
				}

				[Test]
				public void OneKeyIsDocumentArray_ReturnsFalse()
				{
					var inner = new BarbadosDocument.Builder()
						.Add("v", 1)
						.Build();

					var a = new BarbadosDocument.Builder()
						.Add(_field, [inner, inner, inner])
						.Build();

					var b = new BarbadosDocument.Builder()
						.Add(_field, [inner, inner, inner])
						.Build();

					var success = BarbadosDocument.TryCompare(_field, a, b, out _);
					Assert.That(success, Is.False);
				}

				[Test]
				public void SameKeys_SameValues_ReturnsTrue_ResultEqual()
				{
					var a = new BarbadosDocument.Builder()
						.Add(_field, 42)
						.Build();

					var b = new BarbadosDocument.Builder()
						.Add(_field, 42)
						.Build();

					var success = BarbadosDocument.TryCompare(_field, a, b, out var result);
					using (Assert.EnterMultipleScope())
					{
						Assert.That(success, Is.True);
						Assert.That(result, Is.Zero);
					}
				}

				[Test]
				public void DifferentKeys_SameValues_ReturnsTrue_ResultCorrect()
				{
					var a = new BarbadosDocument.Builder()
						.Add("k1", 42)
						.Build();

					var b = new BarbadosDocument.Builder()
						.Add("k2", 42)
						.Build();

					var successA = BarbadosDocument.TryCompare("k1", a, b, out _);
					var successB = BarbadosDocument.TryCompare("k2", a, b, out _);
					using (Assert.EnterMultipleScope())
					{
						Assert.That(successA, Is.False);
						Assert.That(successB, Is.False);
					}
				}

				[Test]
				public void SameKeys_DifferentValues_ReturnsTrue_ResultCorrect()
				{
					var a = new BarbadosDocument.Builder()
						.Add(_field, 1)
						.Build();

					var b = new BarbadosDocument.Builder()
						.Add(_field, 2)
						.Build();

					var success = BarbadosDocument.TryCompare(_field, a, b, out var result);
					using (Assert.EnterMultipleScope())
					{
						Assert.That(success, Is.True);
						Assert.That(result, Is.LessThan(0));
					}
				}

				[Test]
				public void DifferentKeys_DifferentValues_ReturnsTrue_ResultCorrect()
				{
					var a = new BarbadosDocument.Builder()
						.Add("shared", 1)
						.Add("only-a", 10)
						.Build();

					var b = new BarbadosDocument.Builder()
						.Add("shared", 2)
						.Add("only-b", 20)
						.Build();

					var success = BarbadosDocument.TryCompare("shared", a, b, out var result);
					using (Assert.EnterMultipleScope())
					{
						Assert.That(success, Is.True);
						Assert.That(result, Is.LessThan(0));
					}
				}

				[Test]
				[TestCase("v-u8", true)]
				[TestCase("v-i8", true)]
				[TestCase("v-u16", true)]
				[TestCase("v-i16", true)]
				[TestCase("v-u32", true)]
				[TestCase("v-i32", true)]
				[TestCase("v-u64", true)]
				[TestCase("v-i64", true)]
				[TestCase("v-f32", true)]
				[TestCase("v-f64", true)]
				[TestCase("v-dt", true)]
				[TestCase("v-bool", true)]
				[TestCase("v-str", true)]
				[TestCase("v-str-e", true)]
				[TestCase("v-str-u", true)]
				[TestCase("a-i8", true)]
				[TestCase("a-i16", true)]
				[TestCase("a-i32", true)]
				[TestCase("a-i64", true)]
				[TestCase("a-u8", true)]
				[TestCase("a-u16", true)]
				[TestCase("a-u32", true)]
				[TestCase("a-u64", true)]
				[TestCase("a-f32", true)]
				[TestCase("a-f64", true)]
				[TestCase("a-dt", true)]
				[TestCase("a-bool", true)]
				[TestCase("a-str", true)]
				[TestCase("a-str-e", true)]
				[TestCase("a-str-1e", true)]
				[TestCase("巴巴多斯.str.Μπαρμπάντος", true)]
				[TestCase("d1", false)]
				[TestCase("d2", false)]
				public void Omni_ReturnsCorrect_ResultEqual(string key, bool result)
				{
					var success = BarbadosDocument.TryCompare(key, _omniDocument, _omniDocument, out var cmp);
					using (Assert.EnterMultipleScope())
					{
						Assert.That(success, Is.EqualTo(result));
						Assert.That(cmp, Is.Zero);
					}
				}
			}
		}

		public new sealed class Equals
		{
			[Test]
			[SuppressMessage("Assertion", "NUnit2009:The same value has been provided as both the actual and the expected argument", Justification = "")]
			public void EmptyDocuments_ReturnsTrue()
			{
				Assert.That(BarbadosDocument.Empty, Is.EqualTo(BarbadosDocument.Empty));
			}

			[Test]
			[SuppressMessage("Assertion", "NUnit2009:The same value has been provided as both the actual and the expected argument", Justification = "")]
			public void Omni_ReturnsTrue()
			{
				Assert.That(_omniDocument, Is.EqualTo(_omniDocument));
			}
		}
	}
}
