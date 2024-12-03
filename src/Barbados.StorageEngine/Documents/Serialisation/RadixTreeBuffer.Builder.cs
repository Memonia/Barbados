using System;

using Barbados.StorageEngine.Documents.Serialisation.Values;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		public sealed partial class Builder
		{
			public static RadixTreeBuffer EmptyBuffer { get; } = Build(new RadixTreeNode());

			private RadixTreeNode _root;

			public Builder()
			{
				_root = new RadixTreeNode();
			}

			public bool PrefixExists(string prefix)
			{
				var p = new RadixTreePrefix(prefix);
				return _root.TryGet(p.AsSpan(), out _);
			}

			public RadixTreeBuffer Build()
			{
				return Build(_root);
			}

			public void Reset()
			{
				_root = new RadixTreeNode();
			}

			public Builder AddInt8(string name, sbyte value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddInt16(string name, short value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddInt32(string name, int value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddInt64(string name, long value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddUInt8(string name, byte value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddUInt16(string name, ushort value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddUInt32(string name, uint value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddUInt64(string name, ulong value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddFloat32(string name, float value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddFloat64(string name, double value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddBoolean(string name, bool value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddDateTime(string name, DateTime value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddString(string name, string value) => AddBuffer(name, ValueBufferFactory.Create(value));

			public Builder AddInt8Array(string name, sbyte[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddInt16Array(string name, short[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddInt32Array(string name, int[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddInt64Array(string name, long[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddUInt8Array(string name, byte[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddUInt16Array(string name, ushort[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddUInt32Array(string name, uint[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddUInt64Array(string name, ulong[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddFloat32Array(string name, float[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddFloat64Array(string name, double[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddBooleanArray(string name, bool[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddDateTimeArray(string name, DateTime[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddStringArray(string name, string[] values) => AddBuffer(name, ValueBufferFactory.Create(values));

			public Builder AddBuffer(string name, IValueBuffer buffer)
			{
				var p = new RadixTreePrefix(name);
				_root.Add(p.AsSpan(), buffer);
				return this;
			}

			public Builder AddPrefix(string name)
			{
				var p = new RadixTreePrefix(name);
				_root.Add(p.AsSpan(), null);
				return this;
			}
		}
	}
}
