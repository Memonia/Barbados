using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Helpers
{
	internal static class ChainHelpers
	{
		public static void Append<T>(T target, T tail) where T : AbstractPage, ITwoWayChainPage
		{
			tail.Next = target.Header.Handle;
			target.Previous = tail.Header.Handle;
		}

		public static void Prepend<T>(T target, T head) where T : AbstractPage, ITwoWayChainPage
		{
			head.Previous = target.Header.Handle;
			target.Next = head.Header.Handle;
		}

		public static void AppendOneWay<T>(T target, T tail) where T : AbstractPage, IOneWayChainPage
		{
			tail.Next = target.Header.Handle;
		}

		public static void PrependOneWay<T>(T target, T head) where T : AbstractPage, IOneWayChainPage
		{
			head.Next = target.Header.Handle;
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

		public static void RemoveOneWay<T>(T target, T previous) where T : AbstractPage, IOneWayChainPage
		{
			Debug.Assert(previous.Next.Handle == target.Header.Handle.Handle);
			previous.Next = target.Next;
			target.Next = PageHandle.Null;
		}

		public static IEnumerable<T> EnumerateForwardsPinned<T>(TransactionScope transaction, PageHandle start) where T : AbstractPage, IOneWayChainPage
		{
			var next = start;
			while (!next.IsNull)
			{
				var page = transaction.Load<T>(next);
				next = page.Next;

				yield return page;
			}
		}

		public static IEnumerable<T> EnumerateBackwardsPinned<T>(TransactionScope transaction, PageHandle start) where T : AbstractPage, ITwoWayChainPage
		{
			var prev = start;
			while (!prev.IsNull)
			{
				var page = transaction.Load<T>(prev);
				prev = page.Previous;

				yield return page;
			}
		}
	}
}