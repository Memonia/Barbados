using System.Collections.Generic;

using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed class BTreeIndexTraceback
	{
		public int Count => _traceback.Count;
		public bool CanMoveUp => _index > 0;
		public bool CanMoveDown => _index < _traceback.Count - 1;
		public PageHandle Current => _traceback[_index];

		private int _index;
		private readonly List<PageHandle> _traceback;

		public BTreeIndexTraceback(List<PageHandle> traceback) : this(traceback, traceback.Count - 1)
		{

		}

		private BTreeIndexTraceback(List<PageHandle> traceback, int index)
		{
			_index = index;
			_traceback = traceback;
		}

		public bool TryPeekUp(out PageHandle handle)
		{
			if (_index > 0)
			{
				handle = _traceback[_index - 1];
				return true;
			}

			handle = default!;
			return false;
		}

		public bool TryPeekDown(out PageHandle handle)
		{
			if (_index < _traceback.Count - 1)
			{
				handle = _traceback[_index + 1];
				return true;
			}

			handle = default!;
			return false;
		}

		public bool TryMoveUp()
		{
			if (_index > 0)
			{
				_index -= 1;
				return true;
			}

			return false;
		}

		public bool TryMoveDown()
		{
			if (_index < _traceback.Count)
			{
				_index += 1;
				return true;
			}

			return false;
		}

		public void ResetTop()
		{
			_index = 0;
		}

		public void ResetBottom()
		{
			_index = _traceback.Count - 1;
		}

		public BTreeIndexTraceback Clone()
		{
			return new BTreeIndexTraceback(_traceback, _index);
		}
	}
}
