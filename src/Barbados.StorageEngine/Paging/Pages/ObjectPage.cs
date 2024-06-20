using System;
using System.Diagnostics;

using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal sealed partial class ObjectPage : SlottedPage, IBTreeIndexLeaf<ObjectPage>
	{
		private const ushort _headerLength = Constants.PageHandleLength * 2;
		private const ushort _payloadFixedLengthPart = sizeof(int) + Constants.PageHandleLength;

		static ObjectPage()
		{
			DebugHelpers.AssertObjectPageMinObjectCount(_headerLength, _payloadFixedLengthPart);
		}

		public PageHandle Next { get; set; }
		public PageHandle Previous { get; set; }

		/* flags:
		 *  IsChunk
		 * 
		 * payload:
		 *	entry1, entry2, ...
		 *	
		 * entry (IsChunk == false):
		 *	VAR doc
		 *
		 * entry (IsChunk == true):
		 *	I32 total length, PageHandle overflow handle, var chunk
		 */

		public ObjectPage(PageHandle handle) : base(_headerLength, new PageHeader(handle, PageMarker.Object))
		{
			Next = PageHandle.Null;
			Previous = PageHandle.Null;
		}

		public ObjectPage(PageBuffer buffer) : base(buffer)
		{
			if (GetPageMarker(buffer) == PageMarker.Object)
			{
				ReadBaseAndGetStartBufferOffset();
				Debug.Assert(Header.Marker == PageMarker.Object);
			}

			else
			{
				Debug.Assert(GetPageMarker(buffer) == PageMarker.Collection);
			}
		}

		protected ObjectPage(ushort headerLength, PageHeader pageHeader) :
			base((ushort)(headerLength + _headerLength), pageHeader)
		{
			Debug.Assert(_headerLength + headerLength <= ushort.MaxValue);
			DebugHelpers.AssertObjectPageMinObjectCount(
				(ushort)(headerLength + _headerLength), _payloadFixedLengthPart
			);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public bool TryReadLowestId(out ObjectId id)
		{
			if (TryReadFromLowest(out var key, out _, out _))
			{
				id = ObjectIdNormalised.FromNormalised(key);
				return true;
			}

			id = default!;
			return false;
		}

		public bool TryReadHighestId(out ObjectId id)
		{
			if (TryReadFromHighest(out var key, out _, out _))
			{
				id = ObjectIdNormalised.FromNormalised(key);
				return true;
			}

			id = default!;
			return false;
		}

		public bool TryReadObject(ObjectIdNormalised id, out Span<byte> obj)
		{
			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			if (TryRead(key, out obj, out var flags))
			{
				var eflags = new Flags(flags);
				if (!eflags.IsChunk)
				{
					return true;
				}
			}

			obj = default!;
			return false;
		}

		public bool TryWriteObject(ObjectIdNormalised id, ReadOnlySpan<byte> obj)
		{
			if (obj.Length > Constants.ObjectPageMaxChunkLength)
			{
				return false;
			}

			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			if (TryWrite(key, obj))
			{
				var r = TrySetFlags(key, 0);
				Debug.Assert(r);
				return true;
			}

			return false;
		}

		public bool TryReadObjectChunk(ObjectIdNormalised id, out Span<byte> chunk, out int objectLength, out PageHandle overflowHandle)
		{
			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			if (TryRead(key, out var data, out var flags))
			{
				var eflags = new Flags(flags);
				if (eflags.IsChunk)
				{
					var entry = new Chunk(data);
					chunk = entry.Object;
					objectLength = entry.ObjectLength;
					overflowHandle = entry.OverflowHandle;
					return true;
				}
			}

			chunk = default!;
			objectLength = default!;
			overflowHandle = default!;
			return false;
		}

		public bool TryWriteObjectChunk(ObjectIdNormalised id, ReadOnlySpan<byte> obj, out int written)
		{
			var free = SlottedHeader.TotalFreeSpace;
			if (free > sizeof(int) + Constants.PageHandleLength)
			{
				if (free > Constants.ObjectPageMaxChunkLength)
				{
					free = Constants.ObjectPageMaxChunkLength;
				}

				var entryLength = obj.Length + sizeof(int) + Constants.PageHandleLength;
				var toAllocate = entryLength > free
					? free
					: entryLength;

				written = toAllocate - sizeof(int) - Constants.PageHandleLength;
				return _tryWriteObjectChunk(id, obj, written, obj.Length, PageHandle.Null);
			}

			written = default!;
			return false;
		}

		public bool TryWriteObjectChunk(ObjectIdNormalised id, ReadOnlySpan<byte> chunk, int objectLength, PageHandle overflowHandle)
		{
			if (chunk.Length > Constants.ObjectPageMaxChunkLength)
			{
				return false;
			}

			return _tryWriteObjectChunk(id, chunk, chunk.Length, objectLength, overflowHandle);
		}

		public bool TrySetOverfowHandle(ObjectIdNormalised id, PageHandle overflowHandle)
		{
			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			if (TryRead(key, out var data, out var flags))
			{
				var eflags = new Flags(flags);
				if (eflags.IsChunk)
				{
					_ = new Chunk(data)
					{
						OverflowHandle = overflowHandle
					};

					return true;
				}
			}

			return false;
		}

		public bool TryRemoveObject(ObjectIdNormalised id)
		{
			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			if (TryReadObject(id, out _))
			{
				var r = TryRemove(key);
				Debug.Assert(r);
				return true;
			}

			return false;
		}

		public bool TryRemoveObjectChunk(ObjectIdNormalised id, out PageHandle overflowHandle)
		{
			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			if (TryReadObjectChunk(id, out _, out _, out overflowHandle))
			{
				var r = TryRemove(key);
				Debug.Assert(r);
				return true;
			}

			return false;
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			WriteBaseAndGetStartBufferOffset();
			return PageBuffer;
		}

		protected new int ReadBaseAndGetStartBufferOffset()
		{
			var i = base.ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			Next = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;
			Previous = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;

			return i;
		}

		protected new int WriteBaseAndGetStartBufferOffset()
		{
			var i = base.WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			HelpWrite.AsPageHandle(span[i..], Next);
			i += Constants.PageHandleLength;
			HelpWrite.AsPageHandle(span[i..], Previous);
			i += Constants.PageHandleLength;

			return i;
		}

		private bool _tryWriteObjectChunk(ObjectIdNormalised id, ReadOnlySpan<byte> obj, int chunkLength, int objectLength, PageHandle overflowHandle)
		{
			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			var toAllocate = chunkLength + sizeof(int) + Constants.PageHandleLength;
			if (TryAllocate(key, toAllocate, out var span))
			{
				var eflags = new Flags
				{
					IsChunk = true,
				};

				var r = TrySetFlags(key, eflags);
				Debug.Assert(r);

				var entry = new Chunk(span)
				{
					ObjectLength = objectLength,
					OverflowHandle = overflowHandle,
				};

				obj[..chunkLength].CopyTo(entry.Object);
				return true;
			}

			return false;
		}
	}
}
