using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Paging
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
			Debug.Assert(previous.Next.Index == next.Header.Handle.Index);
			Debug.Assert(next.Previous.Index == previous.Header.Handle.Index);

			target.Previous = previous.Header.Handle;
			target.Next = next.Header.Handle;
			previous.Next = target.Header.Handle;
			next.Previous = target.Header.Handle;
		}

		public static void RemoveAndDeallocate<T>(T target, PagePool pool) where T : AbstractPage, ITwoWayChainPage
		{
			if (!target.Previous.IsNull && !target.Next.IsNull)
			{
				var previous = pool.LoadPin<T>(target.Previous);
				var next = pool.LoadPin<T>(target.Next);

				previous.Next = next.Header.Handle;
				next.Previous = previous.Header.Handle;

				pool.SaveRelease(previous);
				pool.SaveRelease(next);
			}

			else
			if (!target.Previous.IsNull)
			{
				var previous = pool.LoadPin<T>(target.Previous);

				previous.Next = PageHandle.Null;
				pool.SaveRelease(previous);
			}

			else
			if (!target.Next.IsNull)
			{
				var next = pool.LoadPin<T>(target.Next);

				next.Previous = PageHandle.Null;
				pool.SaveRelease(next);
			}

			pool.Release(target);
			pool.Deallocate(target.Header.Handle);
		}

		public static void RemoveOneWay<T>(T target, T previous) where T : AbstractPage, IOneWayChainPage
		{
			Debug.Assert(previous.Next.Index == target.Header.Handle.Index);
			previous.Next = target.Next;
			target.Next = PageHandle.Null;
		}

		public static IEnumerable<T> EnumerateForwardsPinned<T>(PagePool pool, PageHandle start, bool release) where T : AbstractPage, IOneWayChainPage
		{
			var next = start;
			while (!next.IsNull)
			{
				var page = pool.LoadPin<T>(next);
				next = page.Next;

				yield return page;
				if (release)
				{
					pool.Release(page);
				}
			}
		}

		public static IEnumerable<T> EnumerateBackwardsPinned<T>(PagePool pool, PageHandle start, bool release) where T : AbstractPage, ITwoWayChainPage
		{
			var prev = start;
			while (!prev.IsNull)
			{
				var page = pool.LoadPin<T>(prev);
				prev = page.Previous;

				yield return page;
				if (release)
				{
					pool.Release(page);
				}
			}
		}
	}
}