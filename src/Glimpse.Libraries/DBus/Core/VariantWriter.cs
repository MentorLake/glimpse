namespace Glimpse.Libraries.DBus.Core;

public static class VariantWriter
{
	public static void WriteDBusVariant(this ref MessageWriter writer, VariantValue value)
	{
		writer.WriteVariant(value);
	}
}
