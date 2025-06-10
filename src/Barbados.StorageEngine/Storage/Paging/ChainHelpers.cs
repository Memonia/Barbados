using System.Diagnostics;

using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Storage.Paging
{
	internal static class ChainHelpers
	{
		public static void Prepend<T>(T target, T head) where T : AbstractPage, ITwoWayChainPage
		{
			head.Previous = target.Header.Handle;
			target.Next = head.Header.Handle;
		}

		public static void Insert<T>(T target, T previous, T next) where T : AbstractPage, ITwoWayChainPage
		{
			Debug.Assert(previous.Next.Handle == next.Header.Handle.Handle);
			Debug.Assert(next.Previous.Handle == previous.Header.Handle.Handle);

			target.Previous = previous.Header.Handle;
			target.Next = next.Header.Handle;
			previous.Next = target.Header.Handle;
			next.Previous = target.Header.Handle;
		}

		public static void RemoveAndDeallocate<T>(T target, TransactionScope transaction) where T : AbstractPage, ITwoWayChainPage
		{
			if (!target.Previous.IsNull && !target.Next.IsNull)
			{
				var previous = transaction.Load<T>(target.Previous);
				var next = transaction.Load<T>(target.Next);

				previous.Next = next.Header.Handle;
				next.Previous = previous.Header.Handle;

				transaction.Save(previous);
				transaction.Save(next);
			}

			else
			if (!target.Previous.IsNull)
			{
				var previous = transaction.Load<T>(target.Previous);

				previous.Next = PageHandle.Null;
				transaction.Save(previous);
			}

			else
			if (!target.Next.IsNull)
			{
				var next = transaction.Load<T>(target.Next);

				next.Previous = PageHandle.Null;
				transaction.Save(next);
			}

			transaction.Deallocate(target.Header.Handle);
		}
	}
}
