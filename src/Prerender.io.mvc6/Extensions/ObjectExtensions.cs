namespace Prerender.io.mvc6.Extensions
{
	public static class ObjectExtensions
	{
		public static bool IsNull(this object o)
		{
			return o == null;
		}

		public static bool IsNotNull(this object o)
		{
			return !IsNull(o);
		}
	}
}