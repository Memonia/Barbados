using System.Collections.Generic;

namespace Barbados.Documents.Tests
{
	public partial class BarbadosDocumentTest
	{
		public sealed class GetKeyEnumerator
		{
			[Test]
			public void EmptyDocument_NoKeys()
			{
				var document = BarbadosDocument.Empty;
				var e = document.GetKeyEnumerator(flat: false);

				Assert.That(e.MoveNext(), Is.False);
			}

			[Test]
			public void EmptyDocument_Flat_NoKeys()
			{
				var document = BarbadosDocument.Empty;
				var e = document.GetKeyEnumerator(flat: true);

				Assert.That(e.MoveNext(), Is.False);
			}

			[Test]
			public void SingleValue_ReturnsKey()
			{
				var document = new BarbadosDocument.Builder()
					.Add(_field, 1)
					.Build();

				var e = document.GetKeyEnumerator(flat: false);
				Assert.That(e.MoveNext(), Is.True);

				var key = e.GetCurrent();
				var hasMore = e.MoveNext();

				using (Assert.EnterMultipleScope())
				{
					Assert.That(key, Is.EqualTo((BarbadosKey)_field));
					Assert.That(hasMore, Is.False);
				}
			}

			[Test]
			public void MultipleValues_ReturnsAllKeys()
			{
				var document = new BarbadosDocument.Builder()
					.Add("a", 1)
					.Add("b", 2)
					.Add("c", 3)
					.Build();

				var keys = new List<string>();
				var e = document.GetKeyEnumerator(flat: false);
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				Assert.Multiple(() =>
				{
					Assert.That(keys, Has.Count.EqualTo(3));
					Assert.That(keys, Does.Contain("a"));
					Assert.That(keys, Does.Contain("b"));
					Assert.That(keys, Does.Contain("c"));
				});
			}

			[Test]
			public void NestedDocument_NotFlat_ReturnsTopLevelKeys()
			{
				var inner = new BarbadosDocument.Builder()
					.Add("v1", 1)
					.Add("v2", 2)
					.Build();

				var document = new BarbadosDocument.Builder()
					.Add("top", "value")
					.Add("doc", inner)
					.Build();

				var keys = new List<string>();
				var e = document.GetKeyEnumerator(flat: false);
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				Assert.Multiple(() =>
				{
					Assert.That(keys, Has.Count.EqualTo(2));
					Assert.That(keys, Does.Contain("top"));
					Assert.That(keys, Does.Contain("doc"));
				});
			}

			[Test]
			public void NestedDocument_Flat_ReturnsAllFlatKeys()
			{
				var inner = new BarbadosDocument.Builder()
					.Add("v1", 1)
					.Add("v2", 2)
					.Build();

				var document = new BarbadosDocument.Builder()
					.Add("top", "value")
					.Add("doc", inner)
					.Build();

				var keys = new List<string>();
				var e = document.GetKeyEnumerator(flat: true);
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				Assert.Multiple(() =>
				{
					Assert.That(keys, Has.Count.EqualTo(3));
					Assert.That(keys, Does.Contain("top"));
					Assert.That(keys, Does.Contain("doc.v1"));
					Assert.That(keys, Does.Contain("doc.v2"));
				});
			}

			[Test]
			public void DeeplyNested_NotFlat_ReturnsTopLevelKeys()
			{
				var document = new BarbadosDocument.Builder()
					.Add("a.b.c", 1)
					.Add("a.b.d", 2)
					.Add("x", 3)
					.Build();

				var keys = new List<string>();
				var e = document.GetKeyEnumerator(flat: false);
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				Assert.Multiple(() =>
				{
					Assert.That(keys, Has.Count.EqualTo(2));
					Assert.That(keys, Does.Contain("a"));
					Assert.That(keys, Does.Contain("x"));
				});
			}

			[Test]
			public void DeeplyNested_Flat_ReturnsAllFlatKeys()
			{
				var document = new BarbadosDocument.Builder()
					.Add("a.b.c", 1)
					.Add("a.b.d", 2)
					.Add("x", 3)
					.Build();

				var keys = new List<string>();
				var e = document.GetKeyEnumerator(flat: true);
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				Assert.Multiple(() =>
				{
					Assert.That(keys, Has.Count.EqualTo(3));
					Assert.That(keys, Does.Contain("a.b.c"));
					Assert.That(keys, Does.Contain("a.b.d"));
					Assert.That(keys, Does.Contain("x"));
				});
			}

			[Test]
			public void DocumentArray_NotFlat_ReturnsTopLevelKey()
			{
				var inner1 = new BarbadosDocument.Builder().Add("v", 1).Build();
				var inner2 = new BarbadosDocument.Builder().Add("v", 2).Build();
				var document = new BarbadosDocument.Builder()
					.Add("arr", [inner1, inner2])
					.Build();

				var keys = new List<string>();
				var e = document.GetKeyEnumerator(flat: false);
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				Assert.That(keys, Has.Count.EqualTo(1));
				Assert.That(keys, Does.Contain("arr"));
			}

			[Test]
			public void Omni_Flat_ReturnsAllKeys()
			{
				var keys = new List<string>();
				var e = _omniDocument.GetKeyEnumerator(flat: true);
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				foreach (var (k, _) in _omniDocumentKV)
				{
					Assert.That(keys, Does.Contain(k));
				}
			}

		}

		public sealed class TryGetDocumentKeyEnumerator
		{
			[Test]
			public void NonExistentKey_ReturnsFalse()
			{
				var document = new BarbadosDocument.Builder()
					.Add(_field, 1)
					.Build();

				var r = document.TryGetDocumentKeyEnumerator("does-not-exist", flat: false, out _);
				Assert.That(r, Is.False);
			}

			[Test]
			public void ValueKey_ReturnsFalse()
			{
				var document = new BarbadosDocument.Builder()
					.Add(_field, 1)
					.Build();

				var r = document.TryGetDocumentKeyEnumerator(_field, flat: false, out _);
				Assert.That(r, Is.False);
			}

			[Test]
			public void DocumentKey_ReturnsTrue()
			{
				var inner = new BarbadosDocument.Builder()
					.Add("v1", 1)
					.Build();

				var document = new BarbadosDocument.Builder()
					.Add("doc", inner)
					.Build();

				var r = document.TryGetDocumentKeyEnumerator("doc", flat: false, out _);
				Assert.That(r, Is.True);
			}

			[Test]
			public void DocumentKey_NotFlat_ReturnsNestedKeys()
			{
				var innerInner = new BarbadosDocument.Builder()
					.Add("deep1", 1)
					.Add("deep2", 2)
					.Build();

				var inner = new BarbadosDocument.Builder()
					.Add("v1", 1)
					.Add("v2", 2)
					.Add("nested", innerInner)
					.Build();

				var document = new BarbadosDocument.Builder()
					.Add("top", "value")
					.Add("doc", inner)
					.Build();

				document.TryGetDocumentKeyEnumerator("doc", flat: false, out var e);
				var keys = new List<string>();
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				Assert.Multiple(() =>
				{
					Assert.That(keys, Has.Count.EqualTo(3));
					Assert.That(keys, Does.Contain("v1"));
					Assert.That(keys, Does.Contain("v2"));
					Assert.That(keys, Does.Contain("nested"));
				});
			}

			[Test]
			public void DocumentKey_Flat_ReturnsAllFlatKeys()
			{
				var innerInner = new BarbadosDocument.Builder()
					.Add("deep", 1)
					.Build();

				var inner = new BarbadosDocument.Builder()
					.Add("v1", 1)
					.Add("nested", innerInner)
					.Build();

				var document = new BarbadosDocument.Builder()
					.Add("doc", inner)
					.Build();

				document.TryGetDocumentKeyEnumerator("doc", flat: true, out var e);
				var keys = new List<string>();
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				Assert.Multiple(() =>
				{
					Assert.That(keys, Has.Count.EqualTo(2));
					Assert.That(keys, Does.Contain("v1"));
					Assert.That(keys, Does.Contain("nested.deep"));
				});
			}

			[Test]
			public void NestedDocumentKey_ReturnsInnerKeys()
			{
				var document = new BarbadosDocument.Builder()
					.Add("d1.d2.v1", 1)
					.Add("d1.d2.v2", 2)
					.Add("d1.v1", 3)
					.Build();

				document.TryGetDocumentKeyEnumerator("d1.d2", flat: false, out var e);
				var keys = new List<string>();
				while (e.MoveNext())
				{
					keys.Add(e.GetCurrent().ToString());
				}

				Assert.Multiple(() =>
				{
					Assert.That(keys, Has.Count.EqualTo(2));
					Assert.That(keys, Does.Contain("v1"));
					Assert.That(keys, Does.Contain("v2"));
				});
			}

			[Test]
			public void EmptyDocument_ReturnsFalse()
			{
				var document = BarbadosDocument.Empty;

				var r = document.TryGetDocumentKeyEnumerator(_field, flat: false, out _);
				Assert.That(r, Is.False);
			}
		}
	}
}
