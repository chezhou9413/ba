using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FisheryLib;

internal static class SpanCompatExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe ref T DangerousGetPinnableReference<T>(this Span<T> span)
	{
		if (span.Length == 0)
			return ref Unsafe.AsRef<T>((void*)1);

		return ref span[0];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe ref T DangerousGetPinnableReference<T>(this ReadOnlySpan<T> span)
	{
		if (span.Length == 0)
			return ref Unsafe.AsRef<T>((void*)1);

		return ref Unsafe.AsRef(in span[0]);
	}
}
