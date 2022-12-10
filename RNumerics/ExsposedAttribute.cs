namespace RNumerics
{
	[System.AttributeUsage(System.AttributeTargets.All)]
	public sealed class ExposedAttribute : System.Attribute
	{
		public ExposedAttribute() {
		}
	}

	[System.AttributeUsage(System.AttributeTargets.All)]
	public sealed class UnExsposedAttribute : System.Attribute
	{
		public UnExsposedAttribute() {
		}
	}

	[System.AttributeUsage(System.AttributeTargets.All)]
	public sealed class NoWriteExsposedAttribute : System.Attribute
	{
		public NoWriteExsposedAttribute() {
		}
	}

}