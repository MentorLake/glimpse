namespace Glimpse.Common.Freedesktop.DBus;

public record DbusObjectDescription
{
	public string ServiceName { get; set; }
	public string ObjectPath { get; set; }
	public string Xml { get; set; }
	public List<DbusInterface> Interfaces { get; set; }

	public bool InterfaceHasMethod(string interfaceQuery, string method)
	{
		var dbusInterface = Interfaces.FirstOrDefault(i => i.Name.Contains(interfaceQuery));
		if (dbusInterface == null) return false;
		return dbusInterface.Methods.Any(m => m == method);
	}
}
