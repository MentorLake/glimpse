using Glimpse.Common.Xfce.SessionManagement;

namespace Glimpse.Common.Configuration;

public class GlimpseAppSettings
{
	public string AppName { get; set; }
	public string ConfigurationFilePath { get; set; }
	public string NotificationsFilePath { get; set; }
	public GlimpseXfceOptions Xfce { get; set; }
}
