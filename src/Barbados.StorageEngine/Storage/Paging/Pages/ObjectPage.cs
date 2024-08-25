using System;
using System.Diagnostics;

using Barbados.StorageEngine.Helpers;

namespace Barbados.StorageEngine.Storage.Paging.Pages
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
			var i = base.ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			Next = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;
			Previous = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;

			Debug.Assert(Header.Marker == PageMarker.Object);
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
			if (TryRead(id, out obj, out var flags))
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

			if (TryWrite(id, obj))
			{
				var r = TrySetFlags(id, 0);
				Debug.Assert(r);
				return true;
			}

			return false;
		}

		public bool TryReadObjectChunk(ObjectIdNormalised id, out Span<byte> chunk, out int objectLength, out PageHandle overflowHandle)
		{
			if (TryRead(id, out var data, out var flags))
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
			if (TryRead(id, out var data, out var flags))
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
			if (TryReadObject(id, out _))
			{
				var r = TryRemove(id);
				Debug.Assert(r);
				return true;
			}

			return false;
		}

		public bool TryRemoveObjectChunk(ObjectIdNormalised id, out PageHandle overflowHandle)
		{
			if (TryReadObjectChunk(id, out _, out _, out overflowHandle))
			{
				var r = TryRemove(id);
				Debug.Assert(r);
				return true;
			}

			return false;
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			var i = base.WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			HelpWrite.AsPageHandle(span[i..], Next);
			i += Constants.PageHandleLength;
			HelpWrite.AsPageHandle(span[i..], Previous);
			i += Constants.PageHandleLength;

			return PageBuffer;
		}

		private bool _tryWriteObjectChunk(ObjectIdNormalised id, ReadOnlySpan<byte> obj, int chunkLength, int objectLength, PageHandle overflowHandle)
		{
			var toAllocate = chunkLength + sizeof(int) + Constants.PageHandleLength;
			if (TryAllocate(id, toAllocate, out var span))
			{
				var eflags = new Flags
				{
					IsChunk = true,
				};

				var r = TrySetFlags(id, eflags);
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
