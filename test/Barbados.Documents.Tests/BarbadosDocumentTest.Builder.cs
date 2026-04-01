using System;
using System.Collections;

namespace Barbados.Documents.Tests
{
	public partial class BarbadosDocumentTest
	{
		public sealed partial class Builder
		{
			public sealed class Add
			{
				private static IEnumerable _case_AncestorsAreValues()
				{
					yield return new TestCaseData("a", "a.b");
					yield return new TestCaseData("a", "a.b.c");
					yield return new TestCaseData("a.b", "a.b.c");
					yield return new TestCaseData(_longKeyValue, $"{_longKeyValue}.b");
					yield return new TestCaseData(_longKeyValue, $"{_longKeyValue}.{_longKeyDocument}");
				}

				private static IEnumerable _case_DescendantIsDocument()
				{
					yield return new TestCaseData("a.b", "a");
					yield return new TestCaseData("a.b.c", "a");
					yield return new TestCaseData("a.b.c", "a.b");
					yield return new TestCaseData($"{_longKeyValue}.b", _longKeyValue);
					yield return new TestCaseData($"{_longKeyValue}.{_longKeyDocument}", _longKeyValue);
				}

				[Test]
				public void Key_Empty_ThrowsException()
				{
					var key = string.Empty;
					var builder = new BarbadosDocument.Builder();
					var value = "value";

					Assert.Throws<ArgumentException>(
						() => builder.Add(key, value)
					);
				}

				[Test]
				public void Key_ValueExists_ThrowsException()
				{
					var key = "duplicate";
					var builder = new BarbadosDocument.Builder();
					var value = "value";

					builder.Add(key, value);
					Assert.Throws<ArgumentException>(
						() => builder.Add(key, value)
					);
				}

				[Test]
				public void Key_DocumentExists_ThrowsException()
				{
					var key = "duplicate";
					var builder = new BarbadosDocument.Builder();
					var value = "value";
					var doc = new BarbadosDocument.Builder().Add(key, value).Build();

					builder.Add(key, doc);
					Assert.Throws<ArgumentException>(
						() => builder.Add(key, value)
					);
				}

				[Test]
				public void Document_Empty_ThrowsException()
				{
					var key = "empty-document";
					var builder = new BarbadosDocument.Builder();
					var document = BarbadosDocument.Empty;

					Assert.Throws<ArgumentException>(
						() => builder.Add(key, document)
					);
				}

				[Test]
				public void DocumentArray_Empty_ThrowsException()
				{
					var key = "empty-array";
					var builder = new BarbadosDocument.Builder();
					var documentArray = Array.Empty<BarbadosDocument>();

					Assert.Throws<ArgumentException>(
						() => builder.Add(key, documentArray)
					);
				}

				[Test]
				public void DocumentArray_WithEmptyDocument_ThrowsException()
				{
					var key = "empty-document-in-array";
					var builder = new BarbadosDocument.Builder();
					var documentArray = new BarbadosDocument[] { BarbadosDocument.Empty };

					Assert.Throws<ArgumentException>(
						() => builder.Add(key, documentArray)
					);
				}

				[TestCaseSource(nameof(_case_AncestorsAreValues))]
				public void Key_AncestorIsValue_AddValue_ThrowsException(string exist, string conflict)
				{
					var builder = new BarbadosDocument.Builder();

					builder.Add(exist, 1);
					Assert.Throws<ArgumentException>(
						() => builder.Add(conflict, 2)
					);
				}

				[TestCaseSource(nameof(_case_AncestorsAreValues))]
				public void Key_AncestorIsValue_AddDocument_ThrowsException(string valueKey, string documentKey)
				{
					var builder = new BarbadosDocument.Builder();
					var nested = new BarbadosDocument.Builder().Add("x", 1).Build();

					builder.Add(valueKey, 1);
					Assert.Throws<ArgumentException>(
						() => builder.Add(documentKey, nested)
					);
				}

				[TestCaseSource(nameof(_case_DescendantIsDocument))]
				public void Key_DescendantIsDocument_AddValue_ThrowsException(string documentKey, string valueKey)
				{
					var builder = new BarbadosDocument.Builder();
					var nested = new BarbadosDocument.Builder().Add("x", 1).Build();

					builder.Add(documentKey, nested);
					Assert.Throws<ArgumentException>(
						() => builder.Add(valueKey, 1)
					);
				}

				[TestCaseSource(nameof(_case_DescendantIsDocument))]
				public void Key_DescendantIsDocument_AddDocument_ThrowsException(string exist, string conflict)
				{
					var builder = new BarbadosDocument.Builder();
					var nested = new BarbadosDocument.Builder().Add("x", 1).Build();

					builder.Add(exist, nested);
					Assert.Throws<ArgumentException>(
						() => builder.Add(conflict, nested)
					);
				}

				[Test]
				public void Key_NestingWithoutConflict_Succeeds()
				{
					var builder = new BarbadosDocument.Builder();
					builder.Add("v.b", 1);
					builder.Add("v.c", 2);
					var doc = builder.Build();

					using (Assert.EnterMultipleScope())
					{
						Assert.That(doc.TryGet("v.b", out _));
						Assert.That(doc.TryGet("v.c", out _));
					}
				}

				[Test]
				public void KeyContainsNestingSeparators_Value_SavedAsNestedDocuments_EachLevelAddressable()
				{
					var value = "value";
					var l1 = "grandparent";
					var l2 = "parent";
					var l3 = "child";
					var l1l2 = $"{l1}{BarbadosKey.NestingSeparator}{l2}";
					var key = $"{l1l2}{BarbadosKey.NestingSeparator}{l3}";
					var doc = new BarbadosDocument.Builder()
						.Add(key, value)
						.Build();

					using (Assert.EnterMultipleScope())
					{
						Assert.That(doc.TryGetDocument(l1, out _));
						Assert.That(doc.TryGetDocument(l1l2, out _));
						Assert.That(doc.TryGet(key, out _));
					}
				}

				[Test]
				public void KeyContainsNestingSeparators_Document_SavedAsNestedDocuments_EachLevelAddressable()
				{
					var nestedField = "nested";
					var nestedFieldValue = "nested-value";
					var l1 = "grandparent";
					var l2 = "parent";
					var l3 = "child";
					var l1l2 = $"{l1}{BarbadosKey.NestingSeparator}{l2}";
					var l1l2l3 = $"{l1l2}{BarbadosKey.NestingSeparator}{l3}";
					var nestedFieldFull = $"{l1l2l3}{BarbadosKey.NestingSeparator}{nestedField}";
					var nestedDoc = new BarbadosDocument.Builder()
						.Add(nestedField, nestedFieldValue)
						.Build();
					var doc = new BarbadosDocument.Builder()
						.Add(l1l2l3, nestedDoc)
						.Build();

					using (Assert.EnterMultipleScope())
					{
						Assert.That(doc.TryGetDocument(l1, out _));
						Assert.That(doc.TryGetDocument(l1l2, out _));
						Assert.That(doc.TryGetDocument(l1l2l3, out _));
						Assert.That(doc.TryGet(nestedFieldFull, out _));
					}
				}

				[Test]
				public void KeyContainsNestingSeparators_DocumentArray_SavedAsNestedDocuments_EachLevelAddressable()
				{
					var nestedField = "nested";
					var nestedFieldValue = "nested-value";
					var l1 = "grandparent";
					var l2 = "parent";
					var l3 = "child";
					var l1l2 = $"{l1}{BarbadosKey.NestingSeparator}{l2}";
					var key = $"{l1l2}{BarbadosKey.NestingSeparator}{l3}";
					var fieldIndex0 = $"{key}{BarbadosKey.NestingSeparator}0";
					var fieldIndex1 = $"{key}{BarbadosKey.NestingSeparator}1";
					var fieldIndex2 = $"{key}{BarbadosKey.NestingSeparator}2";
					var nestedFieldFull1 = $"{fieldIndex0}{BarbadosKey.NestingSeparator}{nestedField}";
					var nestedFieldFull2 = $"{fieldIndex1}{BarbadosKey.NestingSeparator}{nestedField}";
					var nestedFieldFull3 = $"{fieldIndex2}{BarbadosKey.NestingSeparator}{nestedField}";
					var nestedDoc = new BarbadosDocument.Builder()
						.Add(nestedField, nestedFieldValue)
						.Build();
					var doc = new BarbadosDocument.Builder()
						.Add(key, [nestedDoc, nestedDoc, nestedDoc])
						.Build();

					using (Assert.EnterMultipleScope())
					{
						Assert.That(doc.TryGetDocument(l1, out _));
						Assert.That(doc.TryGetDocument(l1l2, out _));
						Assert.That(doc.TryGetDocument(key, out _));
						Assert.That(doc.TryGetDocument(fieldIndex0, out _));
						Assert.That(doc.TryGetDocument(fieldIndex1, out _));
						Assert.That(doc.TryGetDocument(fieldIndex2, out _));
						Assert.That(doc.TryGet(nestedFieldFull1, out _));
						Assert.That(doc.TryGet(nestedFieldFull2, out _));
						Assert.That(doc.TryGet(nestedFieldFull3, out _));
					}
				}
			}

			public sealed class AddFrom
			{
				[Test]
				public void WholeDocument_Empty_ProducesEmptyDocument()
				{
					var source = BarbadosDocument.Empty;
					var document = new BarbadosDocument.Builder()
						.AddFrom(source)
						.Build();

					Assert.That(document.Count(), Is.Zero);
				}

				[Test]
				public void WholeDocument_CombineWithExistingKeys()
				{
					var source = new BarbadosDocument.Builder()
						.Add("from-source", 1)
						.Build();

					var document = new BarbadosDocument.Builder()
						.Add("existing", 2)
						.AddFrom(source)
						.Build();

					using (Assert.EnterMultipleScope())
					{
						Assert.That(document.TryGetInt32("existing", out var existing), Is.True);
						Assert.That(existing, Is.EqualTo(2));
						Assert.That(document.TryGetInt32("from-source", out var fromSource), Is.True);
						Assert.That(fromSource, Is.EqualTo(1));
					}
				}

				[Test]
				public void SpecificKey_ExistingValue_CopiesValue()
				{
					var source = new BarbadosDocument.Builder()
						.Add("a", 1)
						.Add("b", "str")
						.Build();

					var document = new BarbadosDocument.Builder()
						.AddFrom("a", source)
						.Build();

					using (Assert.EnterMultipleScope())
					{
						Assert.That(document.TryGetInt32("a", out var a), Is.True);
						Assert.That(a, Is.EqualTo(1));
						Assert.That(document.Exists("b"), Is.False);
					}
				}

				[Test]
				public void SpecificKey_NonExistent_ThrowsException()
				{
					var source = new BarbadosDocument.Builder()
						.Add("a", 1)
						.Build();

					var builder = new BarbadosDocument.Builder();
					Assert.Throws<ArgumentException>(
						() => builder.AddFrom("does-not-exist", source)
					);
				}

				[Test]
				public void Omni_ProducesEqualDocument()
				{
					var document = new BarbadosDocument.Builder()
						.AddFrom(_omniDocument)
						.Build();

					Assert.That(document, Is.EqualTo(_omniDocument));
				}
			}
		}
	}
}
