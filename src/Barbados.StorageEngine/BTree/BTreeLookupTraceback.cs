using System.Collections.Generic;

using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.BTree
{
	internal sealed class BTreeLookupTraceback
	{
		public int Count => _traceback.Count;
		public bool CanMoveUp => _index > 0;
		public bool CanMoveDown => _index < _traceback.Count - 1;
		public PageHandle Current => _traceback[_index];

		private int _index;
		private readonly List<PageHandle> _traceback;

		public BTreeLookupTraceback(List<PageHandle> traceback) : this(traceback, traceback.Count - 1)
		{

		}

		private BTreeLookupTraceback(List<PageHandle> traceback, int index)
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

		public BTreeLookupTraceback Clone()
		{
			return new BTreeLookupTraceback(_traceback, _index);
		}
	}
}
