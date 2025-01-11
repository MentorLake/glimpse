using System.Reactive.Linq;
using Glimpse.Libraries.DBus.Core;
using Tmds.DBus.Protocol;

namespace Glimpse.Libraries.Accounts;

public class OrgFreedesktopAccountsUser
{
	private const string Interface = "org.freedesktop.Accounts.User";
	private readonly Connection _connection;
	private readonly string _destination;
	private readonly string _path;

	public OrgFreedesktopAccountsUser(Connection connection, string destination, string path)
	{
		_connection = connection;
		_destination = destination;
		_path = path;

		PropertiesChanged = connection.WatchSignal(
			new MatchRule
			{
				Type = MessageType.Signal,
				Sender = destination,
				Path = path,
				Member = "PropertiesChanged",
				Interface = "org.freedesktop.DBus.Properties",
				Arg0 = Interface
			},
			(message, _) =>
			{
				var reader = message.GetBodyReader();
				reader.ReadString();
				List<string> changed = new();
				return new PropertyChanges<Properties>(ReadProperties(ref reader, changed), changed.ToArray(), reader.ReadArray_as());
			})
			.Select(_ => Observable.FromAsync(GetAllPropertiesAsync))
			.Concat();;
	}

	public IObservable<Properties> PropertiesChanged { get; }

	public Task SetUserNameAsync(string name)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetUserName", "s");
			writer.WriteString(name);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetRealNameAsync(string name)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetRealName", "s");
			writer.WriteString(name);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetEmailAsync(string email)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetEmail", "s");
			writer.WriteString(email);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetLanguageAsync(string language)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetLanguage", "s");
			writer.WriteString(language);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetFormatsLocaleAsync(string formats_locale)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetFormatsLocale", "s");
			writer.WriteString(formats_locale);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetInputSourcesAsync(Dictionary<string, string>[] sources)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetInputSources", "aa{ss}");
			writer.WriteArray_aaess(sources);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetXSessionAsync(string x_session)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetXSession", "s");
			writer.WriteString(x_session);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetSessionAsync(string session)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetSession", "s");
			writer.WriteString(session);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetSessionTypeAsync(string session_type)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetSessionType", "s");
			writer.WriteString(session_type);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetLocationAsync(string location)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetLocation", "s");
			writer.WriteString(location);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetHomeDirectoryAsync(string homedir)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetHomeDirectory", "s");
			writer.WriteString(homedir);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetShellAsync(string shell)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetShell", "s");
			writer.WriteString(shell);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetXHasMessagesAsync(bool has_messages)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetXHasMessages", "b");
			writer.WriteBool(has_messages);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetXKeyboardLayoutsAsync(string[] layouts)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetXKeyboardLayouts", "as");
			writer.WriteArray_as(layouts);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetBackgroundFileAsync(string filename)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetBackgroundFile", "s");
			writer.WriteString(filename);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetIconFileAsync(string filename)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetIconFile", "s");
			writer.WriteString(filename);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetLockedAsync(bool locked)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetLocked", "b");
			writer.WriteBool(locked);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetAccountTypeAsync(int accountType)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetAccountType", "i");
			writer.WriteInt32(accountType);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetPasswordModeAsync(int mode)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetPasswordMode", "i");
			writer.WriteInt32(mode);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetPasswordAsync(string password, string hint)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetPassword", "ss");
			writer.WriteString(password);
			writer.WriteString(hint);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetPasswordHintAsync(string hint)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetPasswordHint", "s");
			writer.WriteString(hint);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetAutomaticLoginAsync(bool enabled)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetAutomaticLogin", "b");
			writer.WriteBool(enabled);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<(long expiration_time, long last_change_time, long min_days_between_changes, long max_days_between_changes, long days_to_warn, long days_after_expiration_until_lock)> GetPasswordExpirationPolicyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_xxxxxx);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetPasswordExpirationPolicy");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public ValueTask<IDisposable> WatchChangedAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "Changed",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}

	public Task<ulong> GetUidPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_t);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Uid");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetUserNamePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("UserName");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetRealNamePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("RealName");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<int> GetAccountTypePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_i);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("AccountType");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetHomeDirectoryPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("HomeDirectory");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetShellPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Shell");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetEmailPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Email");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetLanguagePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Language");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetSessionPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Session");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetSessionTypePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("SessionType");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetFormatsLocalePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("FormatsLocale");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<Dictionary<string, string>[]> GetInputSourcesPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_aaess);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("InputSources");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetXSessionPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("XSession");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetLocationPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Location");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<ulong> GetLoginFrequencyPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_t);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("LoginFrequency");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<long> GetLoginTimePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_x);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("LoginTime");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<(long, long, Dictionary<string, DBusVariantItem>)[]> GetLoginHistoryPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_arxxaesvz);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("LoginHistory");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> GetXHasMessagesPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("XHasMessages");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string[]> GetXKeyboardLayoutsPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_as);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("XKeyboardLayouts");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetBackgroundFilePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("BackgroundFile");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetIconFilePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("IconFile");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> GetSavedPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Saved");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> GetLockedPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Locked");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<int> GetPasswordModePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_i);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("PasswordMode");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetPasswordHintPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("PasswordHint");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> GetAutomaticLoginPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("AutomaticLogin");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> GetSystemAccountPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("SystemAccount");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> GetLocalAccountPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("LocalAccount");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<Properties> GetAllPropertiesAsync()
	{
		return _connection.CallMethodAsync(CreateGetAllMessage(), (message, state) =>
		{
			var reader = message.GetBodyReader();
			return ReadProperties(ref reader);
		});

		MessageBuffer CreateGetAllMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "GetAll", "s");
			writer.WriteString(Interface);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	private static Properties ReadProperties(ref Reader reader, List<string>? changed = null)
	{
		var props = new Properties();
		var headersEnd = reader.ReadArrayStart(DBusType.Struct);
		while (reader.HasNext(headersEnd))
		{
			switch (reader.ReadString())
			{
				case "Uid":
					reader.ReadSignature("t");
					props.Uid = reader.ReadUInt64();
					changed?.Add("Uid");
					break;
				case "UserName":
					reader.ReadSignature("s");
					props.UserName = reader.ReadString();
					changed?.Add("UserName");
					break;
				case "RealName":
					reader.ReadSignature("s");
					props.RealName = reader.ReadString();
					changed?.Add("RealName");
					break;
				case "AccountType":
					reader.ReadSignature("i");
					props.AccountType = reader.ReadInt32();
					changed?.Add("AccountType");
					break;
				case "HomeDirectory":
					reader.ReadSignature("s");
					props.HomeDirectory = reader.ReadString();
					changed?.Add("HomeDirectory");
					break;
				case "Shell":
					reader.ReadSignature("s");
					props.Shell = reader.ReadString();
					changed?.Add("Shell");
					break;
				case "Email":
					reader.ReadSignature("s");
					props.Email = reader.ReadString();
					changed?.Add("Email");
					break;
				case "Language":
					reader.ReadSignature("s");
					props.Language = reader.ReadString();
					changed?.Add("Language");
					break;
				case "Session":
					reader.ReadSignature("s");
					props.Session = reader.ReadString();
					changed?.Add("Session");
					break;
				case "SessionType":
					reader.ReadSignature("s");
					props.SessionType = reader.ReadString();
					changed?.Add("SessionType");
					break;
				case "FormatsLocale":
					reader.ReadSignature("s");
					props.FormatsLocale = reader.ReadString();
					changed?.Add("FormatsLocale");
					break;
				case "InputSources":
					reader.ReadSignature("aa{ss}");
					props.InputSources = reader.ReadArray_aaess();
					changed?.Add("InputSources");
					break;
				case "XSession":
					reader.ReadSignature("s");
					props.XSession = reader.ReadString();
					changed?.Add("XSession");
					break;
				case "Location":
					reader.ReadSignature("s");
					props.Location = reader.ReadString();
					changed?.Add("Location");
					break;
				case "LoginFrequency":
					reader.ReadSignature("t");
					props.LoginFrequency = reader.ReadUInt64();
					changed?.Add("LoginFrequency");
					break;
				case "LoginTime":
					reader.ReadSignature("x");
					props.LoginTime = reader.ReadInt64();
					changed?.Add("LoginTime");
					break;
				case "LoginHistory":
					reader.ReadSignature("a(xxa{sv})");
					props.LoginHistory = reader.ReadArray_arxxaesvz();
					changed?.Add("LoginHistory");
					break;
				case "XHasMessages":
					reader.ReadSignature("b");
					props.XHasMessages = reader.ReadBool();
					changed?.Add("XHasMessages");
					break;
				case "XKeyboardLayouts":
					reader.ReadSignature("as");
					props.XKeyboardLayouts = reader.ReadArray_as();
					changed?.Add("XKeyboardLayouts");
					break;
				case "BackgroundFile":
					reader.ReadSignature("s");
					props.BackgroundFile = reader.ReadString();
					changed?.Add("BackgroundFile");
					break;
				case "IconFile":
					reader.ReadSignature("s");
					props.IconFile = reader.ReadString();
					changed?.Add("IconFile");
					break;
				case "Saved":
					reader.ReadSignature("b");
					props.Saved = reader.ReadBool();
					changed?.Add("Saved");
					break;
				case "Locked":
					reader.ReadSignature("b");
					props.Locked = reader.ReadBool();
					changed?.Add("Locked");
					break;
				case "PasswordMode":
					reader.ReadSignature("i");
					props.PasswordMode = reader.ReadInt32();
					changed?.Add("PasswordMode");
					break;
				case "PasswordHint":
					reader.ReadSignature("s");
					props.PasswordHint = reader.ReadString();
					changed?.Add("PasswordHint");
					break;
				case "AutomaticLogin":
					reader.ReadSignature("b");
					props.AutomaticLogin = reader.ReadBool();
					changed?.Add("AutomaticLogin");
					break;
				case "SystemAccount":
					reader.ReadSignature("b");
					props.SystemAccount = reader.ReadBool();
					changed?.Add("SystemAccount");
					break;
				case "LocalAccount":
					reader.ReadSignature("b");
					props.LocalAccount = reader.ReadBool();
					changed?.Add("LocalAccount");
					break;
			}
		}

		return props;
	}

	public ValueTask<IDisposable> WatchPropertiesChangedAsync(Action<Exception?, PropertyChanges<Properties>> handler, bool emitOnCapturedContext = true)
	{
		return SignalHelper.WatchPropertiesChangedAsync(_connection, _destination, _path, Interface, ReadMessage, handler, emitOnCapturedContext);

		static PropertyChanges<Properties> ReadMessage(Message message, object? _)
		{
			var reader = message.GetBodyReader();
			reader.ReadString();
			List<string> changed = new();
			return new PropertyChanges<Properties>(ReadProperties(ref reader, changed), changed.ToArray(), reader.ReadArray_as());
		}
	}

	public class Properties
	{
		public ulong Uid { get; set; }
		public string UserName { get; set; }
		public string RealName { get; set; }
		public int AccountType { get; set; }
		public string HomeDirectory { get; set; }
		public string Shell { get; set; }
		public string Email { get; set; }
		public string Language { get; set; }
		public string Session { get; set; }
		public string SessionType { get; set; }
		public string FormatsLocale { get; set; }
		public Dictionary<string, string>[] InputSources { get; set; }
		public string XSession { get; set; }
		public string Location { get; set; }
		public ulong LoginFrequency { get; set; }
		public long LoginTime { get; set; }
		public (long, long, Dictionary<string, DBusVariantItem>)[] LoginHistory { get; set; }
		public bool XHasMessages { get; set; }
		public string[] XKeyboardLayouts { get; set; }
		public string BackgroundFile { get; set; }
		public string IconFile { get; set; }
		public bool Saved { get; set; }
		public bool Locked { get; set; }
		public int PasswordMode { get; set; }
		public string PasswordHint { get; set; }
		public bool AutomaticLogin { get; set; }
		public bool SystemAccount { get; set; }
		public bool LocalAccount { get; set; }
	}
}
