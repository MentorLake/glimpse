namespace Glimpse.Libraries.DBus.Core;

public static class VariantReader
{
	public static VariantValue ReadDBusVariant(this ref Reader reader)
	{
		reader.ReadSignature();
		return reader.ReadVariantValue();
	}
}
