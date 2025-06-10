using System;
using System.Diagnostics;
using System.Linq;

using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Tests.Storage.Paging
{
	internal sealed partial class SlottedPageTest
	{
		private const int _maxAllocatableLengthWithoutExternalPayload = SlottedPage.PayloadLength - SlottedPage.Descriptor.BinaryLength;

		public sealed class CanAllocate
		{
			[Test]
			[TestCase(false, (ushort)0, SlottedPage.PayloadLength)]
			[TestCase(false, (ushort)45, SlottedPage.PayloadLength - 45)]
			[TestCase(true, (ushort)0, _maxAllocatableLengthWithoutExternalPayload)]
			[TestCase(true, (ushort)45, _maxAllocatableLengthWithoutExternalPayload - 45)]
			public void EmptyPage_WithExternalPayload_CanAllocateLength(
				bool expectedResult, ushort externalPayloadLength, int allocationLength
			)
			{
				Debug.Assert(externalPayloadLength >= 0 && allocationLength > 0);

				var page = new SlottedPageFake(externalPayloadLength);
				var canAllocate = page.CanAllocate(allocationLength, 0);
				Assert.That(canAllocate, Is.EqualTo(expectedResult));
			}

			[Test]
			// TODO: make this case work
			// [TestCase(true, _maxAllocatableLengthWithoutExternalPayload, _maxAllocatableLengthWithoutExternalPayload)]
			[TestCase(true, 128, 128)]
			[TestCase(true, _maxAllocatableLengthWithoutExternalPayload, 128)]
			public void EmptyPage_InsertLengthThenRemove_CanAllocateLength(
				bool expectedResult, int insertionLength, int allocationLength
			)
			{
				Debug.Assert(insertionLength - 1 > 0 && allocationLength > 0);

				var k = new byte[] { 1 };
				var d = Enumerable.Range(1, insertionLength - 1).Select(e => (byte)(e % byte.MaxValue)).ToArray();
				var page = new SlottedPageFake(0);
				var r = page.TryWrite(k, d);
				Assert.That(r, Is.True, "Failed to write data to the page");

				r = page.TryRemove(k);
				Assert.That(r, Is.True, "Failed to remove data from the page");

				var canAllocate = page.CanAllocate(allocationLength, 0);
				Assert.That(canAllocate, Is.EqualTo(expectedResult));
			}
		}

		public sealed class TryRead
		{
			[Test]
			public void NonEmptyPage_EmptyKey_Fails()
			{
				var page = new SlottedPageFake(0);
				var r = page.TryWrite([1, 2, 3], [1, 2, 3]);
				Assert.That(r, Is.True, "Failed to write data to the page");

				r = page.TryRead([], out _, out _);
				Assert.That(r, Is.False);
			}

			[Test]
			public void NonEmptyPage_ZeroedBytesKey_Fails()
			{
				var page = new SlottedPageFake(0);
				var k = new byte[] { 0, 0, 0 };

				var r = page.TryWrite([1, 2, 3], [1, 2, 3]);
				Assert.That(r, Is.True, "Failed to write data to the page");

				r = page.TryRead(k, out _, out _);
				Assert.That(r, Is.False);
			}
		}

		public sealed class TryWrite
		{
			[Test]
			public void WriteBytes_ReadBack_BytesMatch()
			{
				var page = new SlottedPageFake(0);
				var k = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
				var d = new byte[] { 7, 6, 5, 4, 3, 2, 1, 0 };

				var r = page.TryWrite(k, d);
				Assert.That(r, Is.True, "Failed to write data to the page");

				r = page.TryRead(k, out var data, out var flags);
				var dataArr = data.ToArray();
				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True, "Failed to read data from the page");
					Assert.That(flags, Is.Zero, "No flags should be set for a new entry");
					Assert.That(dataArr.SequenceEqual(d), "Read data does not match written data");
				});
			}
		}

		public sealed class TryRemove
		{
			[Test]
			public void KeyExists_Success()
			{
				var page = new SlottedPageFake(0);
				var k = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
				var d = new byte[] { 7, 6, 5, 4, 3, 2, 1, 0 };
				var r = page.TryWrite(k, d);
				Assert.That(r, Is.True, "Failed to write data to the page");

				r = page.TryRemove(k);
				Assert.That(r, Is.True);

				r = page.TryRead(k, out _, out _);
				Assert.That(r, Is.False, "Data was not removed from the page");
			}

			[Test]
			public void KeyDoesNotExist_Fails()
			{
				var page = new SlottedPageFake(0);
				var k = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
				var d = new byte[] { 7, 6, 5, 4, 3, 2, 1, 0 };
				var r = page.TryWrite(k, d);
				Assert.That(r, Is.True, "Failed to write data to the page");

				k[0] = 16;
				r = page.TryRemove(k);
				Assert.That(r, Is.False);

				k[0] = 0;
				r = page.TryRead(k, out _, out _);
				Assert.That(r, Is.True, "Existing data was removed");
			}
		}

		public sealed class TrySetFlags
		{
			[Test]
			public void KeyExists_CorrectFlagsReadBack()
			{
				var page = new SlottedPageFake(0);
				var k = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
				var d = new byte[] { 7, 6, 5, 4, 3, 2, 1, 0 };
				var f = (byte)45;
				var r = page.TryWrite(k, d);
				Assert.That(r, Is.True, "Failed to write data to the page");

				r = page.TrySetFlags(k, f);
				Assert.That(r, Is.True, "Fail to set the flags");

				r = page.TryRead(k, out var data, out var flags);
				var dataArr = data.ToArray();
				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True, "Failed to read data from the page");
					Assert.That(flags, Is.EqualTo(f), "Flags were not set correctly");
					Assert.That(dataArr.SequenceEqual(d), "Read data does not match written data");
				});
			}

			[Test]
			public void KeyDoesNotExist_Fails()
			{
				var page = new SlottedPageFake(0);
				var k = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
				var r = page.TrySetFlags(k, 45);
				Assert.That(r, Is.False);
			}
		}

		public sealed class TryAllocate
		{
			[Test]
			public void EmptyPage_AllocateLength_CorrectLengthAllocated()
			{
				var page = new SlottedPageFake(0);
				var k = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
				var d = new byte[128];
				var r = page.TryAllocate(k, d.Length, out var allocated);
				var allocatedLength = allocated.Length;
				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True, "Failed to allocate data on the page");
					Assert.That(allocatedLength, Is.EqualTo(d.Length), "Allocated data length does not match expected length");
				});
			}
		}

		[Test]
		public void WriteBytes_RemoveBytes_WriteMoreBytes_CompactionTriggered_WriteSucceeded()
		{
			var page = new SlottedPageFake(0);
			var k1 = new byte[] { 1 };
			var k2 = new byte[] { 2 };
			var d1 = new byte[_maxAllocatableLengthWithoutExternalPayload / 2 - SlottedPage.Descriptor.BinaryLength];
			var d2 = new byte[_maxAllocatableLengthWithoutExternalPayload / 2 - SlottedPage.Descriptor.BinaryLength];

			var k3 = new byte[] { 3 };
			var d3 = new byte[_maxAllocatableLengthWithoutExternalPayload - 1];

			var a = page.TryWrite(k1, d1);
			var b = page.TryWrite(k2, d2);
			var c = page.TryRemove(k1);
			var d = page.TryRemove(k2);
			Assert.Multiple(() =>
			{
				Assert.That(new bool[] { a, b, c, d }, Has.All.True);
				Assert.That(page.CanCompact, Is.True);
			});

			var r = page.TryWrite(k3, d3);
			Assert.Multiple(() =>
			{
				Assert.That(r, Is.True);
				Assert.That(page.CanCompact, Is.False);
			});
		}
	}
}
