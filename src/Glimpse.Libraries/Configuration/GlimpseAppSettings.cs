using Glimpse.Libraries.Xfce.SessionManagement;

namespace Glimpse.Libraries.Configuration;

public class GlimpseAppSettings
{
	public string ApplicationName { get; set; }
	public string ConfigurationFilePath { get; set; }
	public string NotificationsFilePath { get; set; }
	public GlimpseXfceOptions Xfce { get; set; }
}
