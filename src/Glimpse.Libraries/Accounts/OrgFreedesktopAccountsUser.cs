using Tmds.DBus.Protocol;

namespace Glimpse.Libraries.Accounts;

record AccountsProperties
{
	public string DaemonVersion { get; set; } = default!;
	public bool HasNoUsers { get; set; } = default!;
	public bool HasMultipleUsers { get; set; } = default!;
	public ObjectPath[] AutomaticLoginUsers { get; set; } = default!;
}
partial class Accounts : AccountsObject
{
	private const string __Interface = "org.freedesktop.Accounts";
	public Accounts(AccountsDbusServiceFactory dbusServiceFactory, ObjectPath path) : base(dbusServiceFactory, path)
	{ }
	public Task<ObjectPath[]> ListCachedUsersAsync()
	{
		return this.Connection.CallMethodAsync(CreateMessage(), (Message m, object? s) => ReadMessage_ao(m, (AccountsObject)s!), this);
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				member: "ListCachedUsers");
			return writer.CreateMessage();
		}
	}
	public Task<ObjectPath> FindUserByIdAsync(long id)
	{
		return this.Connection.CallMethodAsync(CreateMessage(), (Message m, object? s) => ReadMessage_o(m, (AccountsObject)s!), this);
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "x",
				member: "FindUserById");
			writer.WriteInt64(id);
			return writer.CreateMessage();
		}
	}
	public Task<ObjectPath> FindUserByNameAsync(string name)
	{
		return this.Connection.CallMethodAsync(CreateMessage(), (Message m, object? s) => ReadMessage_o(m, (AccountsObject)s!), this);
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "FindUserByName");
			writer.WriteString(name);
			return writer.CreateMessage();
		}
	}
	public Task<ObjectPath> CreateUserAsync(string name, string fullname, int accountType)
	{
		return this.Connection.CallMethodAsync(CreateMessage(), (Message m, object? s) => ReadMessage_o(m, (AccountsObject)s!), this);
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "ssi",
				member: "CreateUser");
			writer.WriteString(name);
			writer.WriteString(fullname);
			writer.WriteInt32(accountType);
			return writer.CreateMessage();
		}
	}
	public Task<ObjectPath> CacheUserAsync(string name)
	{
		return this.Connection.CallMethodAsync(CreateMessage(), (Message m, object? s) => ReadMessage_o(m, (AccountsObject)s!), this);
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "CacheUser");
			writer.WriteString(name);
			return writer.CreateMessage();
		}
	}
	public Task UncacheUserAsync(string name)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "UncacheUser");
			writer.WriteString(name);
			return writer.CreateMessage();
		}
	}
	public Task DeleteUserAsync(long id, bool removeFiles)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "xb",
				member: "DeleteUser");
			writer.WriteInt64(id);
			writer.WriteBool(removeFiles);
			return writer.CreateMessage();
		}
	}
	public Task<string[]> GetUsersLanguagesAsync()
	{
		return this.Connection.CallMethodAsync(CreateMessage(), (Message m, object? s) => ReadMessage_as(m, (AccountsObject)s!), this);
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				member: "GetUsersLanguages");
			return writer.CreateMessage();
		}
	}
	public ValueTask<IDisposable> WatchUserAddedAsync(Action<Exception?, ObjectPath> handler, bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
		=> base.WatchSignalAsync(DbusServiceFactory.Destination, __Interface, Path, "UserAdded", (Message m, object? s) => ReadMessage_o(m, (AccountsObject)s!), handler, emitOnCapturedContext, flags);
	public ValueTask<IDisposable> WatchUserDeletedAsync(Action<Exception?, ObjectPath> handler, bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
		=> base.WatchSignalAsync(DbusServiceFactory.Destination, __Interface, Path, "UserDeleted", (Message m, object? s) => ReadMessage_o(m, (AccountsObject)s!), handler, emitOnCapturedContext, flags);
	public Task<string> GetDaemonVersionAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "DaemonVersion"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<bool> GetHasNoUsersAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "HasNoUsers"), (Message m, object? s) => ReadMessage_v_b(m, (AccountsObject)s!), this);
	public Task<bool> GetHasMultipleUsersAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "HasMultipleUsers"), (Message m, object? s) => ReadMessage_v_b(m, (AccountsObject)s!), this);
	public Task<ObjectPath[]> GetAutomaticLoginUsersAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "AutomaticLoginUsers"), (Message m, object? s) => ReadMessage_v_ao(m, (AccountsObject)s!), this);
	public Task<AccountsProperties> GetPropertiesAsync()
	{
		return this.Connection.CallMethodAsync(CreateGetAllPropertiesMessage(__Interface), (Message m, object? s) => ReadMessage(m, (AccountsObject)s!), this);
		static AccountsProperties ReadMessage(Message message, AccountsObject _)
		{
			var reader = message.GetBodyReader();
			return ReadProperties(ref reader);
		}
	}
	public ValueTask<IDisposable> WatchPropertiesChangedAsync(Action<Exception?, PropertyChanges<AccountsProperties>> handler, bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
	{
		return base.WatchPropertiesChangedAsync(__Interface, (Message m, object? s) => ReadMessage(m, (AccountsObject)s!), handler, emitOnCapturedContext, flags);
		static PropertyChanges<AccountsProperties> ReadMessage(Message message, AccountsObject _)
		{
			var reader = message.GetBodyReader();
			reader.ReadString(); // interface
			List<string> changed = new(), invalidated = new();
			return new PropertyChanges<AccountsProperties>(ReadProperties(ref reader, changed), ReadInvalidated(ref reader), changed.ToArray());
		}
		static string[] ReadInvalidated(ref Reader reader)
		{
			List<string>? invalidated = null;
			ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.String);
			while (reader.HasNext(arrayEnd))
			{
				invalidated ??= new();
				var property = reader.ReadString();
				switch (property)
				{
					case "DaemonVersion": invalidated.Add("DaemonVersion"); break;
					case "HasNoUsers": invalidated.Add("HasNoUsers"); break;
					case "HasMultipleUsers": invalidated.Add("HasMultipleUsers"); break;
					case "AutomaticLoginUsers": invalidated.Add("AutomaticLoginUsers"); break;
				}
			}
			return invalidated?.ToArray() ?? Array.Empty<string>();
		}
	}
	private static AccountsProperties ReadProperties(ref Reader reader, List<string>? changedList = null)
	{
		var props = new AccountsProperties();
		ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.Struct);
		while (reader.HasNext(arrayEnd))
		{
			var property = reader.ReadString();
			switch (property)
			{
				case "DaemonVersion":
					reader.ReadSignature("s"u8);
					props.DaemonVersion = reader.ReadString();
					changedList?.Add("DaemonVersion");
					break;
				case "HasNoUsers":
					reader.ReadSignature("b"u8);
					props.HasNoUsers = reader.ReadBool();
					changedList?.Add("HasNoUsers");
					break;
				case "HasMultipleUsers":
					reader.ReadSignature("b"u8);
					props.HasMultipleUsers = reader.ReadBool();
					changedList?.Add("HasMultipleUsers");
					break;
				case "AutomaticLoginUsers":
					reader.ReadSignature("ao"u8);
					props.AutomaticLoginUsers = reader.ReadArrayOfObjectPath();
					changedList?.Add("AutomaticLoginUsers");
					break;
				default:
					reader.ReadVariantValue();
					break;
			}
		}
		return props;
	}
}
record UserProperties
{
	public ulong Uid { get; set; } = default!;
	public string UserName { get; set; } = default!;
	public string RealName { get; set; } = default!;
	public int AccountType { get; set; } = default!;
	public string HomeDirectory { get; set; } = default!;
	public string Shell { get; set; } = default!;
	public string Email { get; set; } = default!;
	public string Language { get; set; } = default!;
	public string[] Languages { get; set; } = default!;
	public string Session { get; set; } = default!;
	public string SessionType { get; set; } = default!;
	public string FormatsLocale { get; set; } = default!;
	public Dictionary<string, string>[] InputSources { get; set; } = default!;
	public string XSession { get; set; } = default!;
	public string Location { get; set; } = default!;
	public ulong LoginFrequency { get; set; } = default!;
	public long LoginTime { get; set; } = default!;
	public (long, long, Dictionary<string, VariantValue>)[] LoginHistory { get; set; } = default!;
	public bool XHasMessages { get; set; } = default!;
	public string[] XKeyboardLayouts { get; set; } = default!;
	public string BackgroundFile { get; set; } = default!;
	public string IconFile { get; set; } = default!;
	public bool Saved { get; set; } = default!;
	public bool Locked { get; set; } = default!;
	public int PasswordMode { get; set; } = default!;
	public string PasswordHint { get; set; } = default!;
	public bool AutomaticLogin { get; set; } = default!;
	public bool SystemAccount { get; set; } = default!;
	public bool LocalAccount { get; set; } = default!;
}
partial class User : AccountsObject
{
	private const string __Interface = "org.freedesktop.Accounts.User";
	public User(AccountsDbusServiceFactory dbusServiceFactory, ObjectPath path) : base(dbusServiceFactory, path)
	{ }
	public Task SetUserNameAsync(string name)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetUserName");
			writer.WriteString(name);
			return writer.CreateMessage();
		}
	}
	public Task SetRealNameAsync(string name)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetRealName");
			writer.WriteString(name);
			return writer.CreateMessage();
		}
	}
	public Task SetEmailAsync(string email)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetEmail");
			writer.WriteString(email);
			return writer.CreateMessage();
		}
	}
	public Task SetLanguageAsync(string language)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetLanguage");
			writer.WriteString(language);
			return writer.CreateMessage();
		}
	}
	public Task SetLanguagesAsync(string[] languages)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "as",
				member: "SetLanguages");
			writer.WriteArray(languages);
			return writer.CreateMessage();
		}
	}
	public Task SetFormatsLocaleAsync(string formatsLocale)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetFormatsLocale");
			writer.WriteString(formatsLocale);
			return writer.CreateMessage();
		}
	}
	public Task SetInputSourcesAsync(Dictionary<string, string>[] sources)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "aa{ss}",
				member: "SetInputSources");
			WriteType_aaess(ref writer, sources);
			return writer.CreateMessage();
		}
	}
	public Task SetXSessionAsync(string xSession)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetXSession");
			writer.WriteString(xSession);
			return writer.CreateMessage();
		}
	}
	public Task SetSessionAsync(string session)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetSession");
			writer.WriteString(session);
			return writer.CreateMessage();
		}
	}
	public Task SetSessionTypeAsync(string sessionType)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetSessionType");
			writer.WriteString(sessionType);
			return writer.CreateMessage();
		}
	}
	public Task SetLocationAsync(string location)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetLocation");
			writer.WriteString(location);
			return writer.CreateMessage();
		}
	}
	public Task SetHomeDirectoryAsync(string homedir)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetHomeDirectory");
			writer.WriteString(homedir);
			return writer.CreateMessage();
		}
	}
	public Task SetShellAsync(string shell)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetShell");
			writer.WriteString(shell);
			return writer.CreateMessage();
		}
	}
	public Task SetXHasMessagesAsync(bool hasMessages)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "b",
				member: "SetXHasMessages");
			writer.WriteBool(hasMessages);
			return writer.CreateMessage();
		}
	}
	public Task SetXKeyboardLayoutsAsync(string[] layouts)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "as",
				member: "SetXKeyboardLayouts");
			writer.WriteArray(layouts);
			return writer.CreateMessage();
		}
	}
	public Task SetBackgroundFileAsync(string filename)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetBackgroundFile");
			writer.WriteString(filename);
			return writer.CreateMessage();
		}
	}
	public Task SetIconFileAsync(string filename)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetIconFile");
			writer.WriteString(filename);
			return writer.CreateMessage();
		}
	}
	public Task SetLockedAsync(bool locked)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "b",
				member: "SetLocked");
			writer.WriteBool(locked);
			return writer.CreateMessage();
		}
	}
	public Task SetAccountTypeAsync(int accountType)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "i",
				member: "SetAccountType");
			writer.WriteInt32(accountType);
			return writer.CreateMessage();
		}
	}
	public Task SetPasswordModeAsync(int mode)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "i",
				member: "SetPasswordMode");
			writer.WriteInt32(mode);
			return writer.CreateMessage();
		}
	}
	public Task SetPasswordAsync(string password, string hint)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "ss",
				member: "SetPassword");
			writer.WriteString(password);
			writer.WriteString(hint);
			return writer.CreateMessage();
		}
	}
	public Task SetPasswordHintAsync(string hint)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "s",
				member: "SetPasswordHint");
			writer.WriteString(hint);
			return writer.CreateMessage();
		}
	}
	public Task SetAutomaticLoginAsync(bool enabled)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "b",
				member: "SetAutomaticLogin");
			writer.WriteBool(enabled);
			return writer.CreateMessage();
		}
	}
	public Task<(long ExpirationTime, long LastChangeTime, long MinDaysBetweenChanges, long MaxDaysBetweenChanges, long DaysToWarn, long DaysAfterExpirationUntilLock)> GetPasswordExpirationPolicyAsync()
	{
		return this.Connection.CallMethodAsync(CreateMessage(), (Message m, object? s) => ReadMessage_xxxxxx(m, (AccountsObject)s!), this);
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				member: "GetPasswordExpirationPolicy");
			return writer.CreateMessage();
		}
	}
	public Task SetPasswordExpirationPolicyAsync(long minDaysBetweenChanges, long maxDaysBetweenChanges, long daysToWarn, long daysAfterExpirationUntilLock)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "xxxx",
				member: "SetPasswordExpirationPolicy");
			writer.WriteInt64(minDaysBetweenChanges);
			writer.WriteInt64(maxDaysBetweenChanges);
			writer.WriteInt64(daysToWarn);
			writer.WriteInt64(daysAfterExpirationUntilLock);
			return writer.CreateMessage();
		}
	}
	public Task SetUserExpirationPolicyAsync(long expirationTime)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: __Interface,
				signature: "x",
				member: "SetUserExpirationPolicy");
			writer.WriteInt64(expirationTime);
			return writer.CreateMessage();
		}
	}
	public ValueTask<IDisposable> WatchChangedAsync(Action<Exception?> handler, bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
		=> base.WatchSignalAsync(DbusServiceFactory.Destination, __Interface, Path, "Changed", handler, emitOnCapturedContext, flags);
	public Task<ulong> GetUidAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "Uid"), (Message m, object? s) => ReadMessage_v_t(m, (AccountsObject)s!), this);
	public Task<string> GetUserNameAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "UserName"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<string> GetRealNameAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "RealName"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<int> GetAccountTypeAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "AccountType"), (Message m, object? s) => ReadMessage_v_i(m, (AccountsObject)s!), this);
	public Task<string> GetHomeDirectoryAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "HomeDirectory"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<string> GetShellAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "Shell"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<string> GetEmailAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "Email"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<string> GetLanguageAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "Language"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<string[]> GetLanguagesAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "Languages"), (Message m, object? s) => ReadMessage_v_as(m, (AccountsObject)s!), this);
	public Task<string> GetSessionAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "Session"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<string> GetSessionTypeAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "SessionType"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<string> GetFormatsLocaleAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "FormatsLocale"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<Dictionary<string, string>[]> GetInputSourcesAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "InputSources"), (Message m, object? s) => ReadMessage_v_aaess(m, (AccountsObject)s!), this);
	public Task<string> GetXSessionAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "XSession"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<string> GetLocationAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "Location"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<ulong> GetLoginFrequencyAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "LoginFrequency"), (Message m, object? s) => ReadMessage_v_t(m, (AccountsObject)s!), this);
	public Task<long> GetLoginTimeAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "LoginTime"), (Message m, object? s) => ReadMessage_v_x(m, (AccountsObject)s!), this);
	public Task<(long, long, Dictionary<string, VariantValue>)[]> GetLoginHistoryAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "LoginHistory"), (Message m, object? s) => ReadMessage_v_arxxaesvz(m, (AccountsObject)s!), this);
	public Task<bool> GetXHasMessagesAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "XHasMessages"), (Message m, object? s) => ReadMessage_v_b(m, (AccountsObject)s!), this);
	public Task<string[]> GetXKeyboardLayoutsAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "XKeyboardLayouts"), (Message m, object? s) => ReadMessage_v_as(m, (AccountsObject)s!), this);
	public Task<string> GetBackgroundFileAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "BackgroundFile"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<string> GetIconFileAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "IconFile"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<bool> GetSavedAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "Saved"), (Message m, object? s) => ReadMessage_v_b(m, (AccountsObject)s!), this);
	public Task<bool> GetLockedAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "Locked"), (Message m, object? s) => ReadMessage_v_b(m, (AccountsObject)s!), this);
	public Task<int> GetPasswordModeAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "PasswordMode"), (Message m, object? s) => ReadMessage_v_i(m, (AccountsObject)s!), this);
	public Task<string> GetPasswordHintAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "PasswordHint"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<bool> GetAutomaticLoginAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "AutomaticLogin"), (Message m, object? s) => ReadMessage_v_b(m, (AccountsObject)s!), this);
	public Task<bool> GetSystemAccountAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "SystemAccount"), (Message m, object? s) => ReadMessage_v_b(m, (AccountsObject)s!), this);
	public Task<bool> GetLocalAccountAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "LocalAccount"), (Message m, object? s) => ReadMessage_v_b(m, (AccountsObject)s!), this);
	public Task<UserProperties> GetPropertiesAsync()
	{
		return this.Connection.CallMethodAsync(CreateGetAllPropertiesMessage(__Interface), (Message m, object? s) => ReadMessage(m, (AccountsObject)s!), this);
		static UserProperties ReadMessage(Message message, AccountsObject _)
		{
			var reader = message.GetBodyReader();
			return ReadProperties(ref reader);
		}
	}
	public ValueTask<IDisposable> WatchPropertiesChangedAsync(Action<Exception?, PropertyChanges<UserProperties>> handler, bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
	{
		return base.WatchPropertiesChangedAsync(__Interface, (Message m, object? s) => ReadMessage(m, (AccountsObject)s!), handler, emitOnCapturedContext, flags);
		static PropertyChanges<UserProperties> ReadMessage(Message message, AccountsObject _)
		{
			var reader = message.GetBodyReader();
			reader.ReadString(); // interface
			List<string> changed = new(), invalidated = new();
			return new PropertyChanges<UserProperties>(ReadProperties(ref reader, changed), ReadInvalidated(ref reader), changed.ToArray());
		}
		static string[] ReadInvalidated(ref Reader reader)
		{
			List<string>? invalidated = null;
			ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.String);
			while (reader.HasNext(arrayEnd))
			{
				invalidated ??= new();
				var property = reader.ReadString();
				switch (property)
				{
					case "Uid": invalidated.Add("Uid"); break;
					case "UserName": invalidated.Add("UserName"); break;
					case "RealName": invalidated.Add("RealName"); break;
					case "AccountType": invalidated.Add("AccountType"); break;
					case "HomeDirectory": invalidated.Add("HomeDirectory"); break;
					case "Shell": invalidated.Add("Shell"); break;
					case "Email": invalidated.Add("Email"); break;
					case "Language": invalidated.Add("Language"); break;
					case "Languages": invalidated.Add("Languages"); break;
					case "Session": invalidated.Add("Session"); break;
					case "SessionType": invalidated.Add("SessionType"); break;
					case "FormatsLocale": invalidated.Add("FormatsLocale"); break;
					case "InputSources": invalidated.Add("InputSources"); break;
					case "XSession": invalidated.Add("XSession"); break;
					case "Location": invalidated.Add("Location"); break;
					case "LoginFrequency": invalidated.Add("LoginFrequency"); break;
					case "LoginTime": invalidated.Add("LoginTime"); break;
					case "LoginHistory": invalidated.Add("LoginHistory"); break;
					case "XHasMessages": invalidated.Add("XHasMessages"); break;
					case "XKeyboardLayouts": invalidated.Add("XKeyboardLayouts"); break;
					case "BackgroundFile": invalidated.Add("BackgroundFile"); break;
					case "IconFile": invalidated.Add("IconFile"); break;
					case "Saved": invalidated.Add("Saved"); break;
					case "Locked": invalidated.Add("Locked"); break;
					case "PasswordMode": invalidated.Add("PasswordMode"); break;
					case "PasswordHint": invalidated.Add("PasswordHint"); break;
					case "AutomaticLogin": invalidated.Add("AutomaticLogin"); break;
					case "SystemAccount": invalidated.Add("SystemAccount"); break;
					case "LocalAccount": invalidated.Add("LocalAccount"); break;
				}
			}
			return invalidated?.ToArray() ?? Array.Empty<string>();
		}
	}
	private static UserProperties ReadProperties(ref Reader reader, List<string>? changedList = null)
	{
		var props = new UserProperties();
		ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.Struct);
		while (reader.HasNext(arrayEnd))
		{
			var property = reader.ReadString();
			switch (property)
			{
				case "Uid":
					reader.ReadSignature("t"u8);
					props.Uid = reader.ReadUInt64();
					changedList?.Add("Uid");
					break;
				case "UserName":
					reader.ReadSignature("s"u8);
					props.UserName = reader.ReadString();
					changedList?.Add("UserName");
					break;
				case "RealName":
					reader.ReadSignature("s"u8);
					props.RealName = reader.ReadString();
					changedList?.Add("RealName");
					break;
				case "AccountType":
					reader.ReadSignature("i"u8);
					props.AccountType = reader.ReadInt32();
					changedList?.Add("AccountType");
					break;
				case "HomeDirectory":
					reader.ReadSignature("s"u8);
					props.HomeDirectory = reader.ReadString();
					changedList?.Add("HomeDirectory");
					break;
				case "Shell":
					reader.ReadSignature("s"u8);
					props.Shell = reader.ReadString();
					changedList?.Add("Shell");
					break;
				case "Email":
					reader.ReadSignature("s"u8);
					props.Email = reader.ReadString();
					changedList?.Add("Email");
					break;
				case "Language":
					reader.ReadSignature("s"u8);
					props.Language = reader.ReadString();
					changedList?.Add("Language");
					break;
				case "Languages":
					reader.ReadSignature("as"u8);
					props.Languages = reader.ReadArrayOfString();
					changedList?.Add("Languages");
					break;
				case "Session":
					reader.ReadSignature("s"u8);
					props.Session = reader.ReadString();
					changedList?.Add("Session");
					break;
				case "SessionType":
					reader.ReadSignature("s"u8);
					props.SessionType = reader.ReadString();
					changedList?.Add("SessionType");
					break;
				case "FormatsLocale":
					reader.ReadSignature("s"u8);
					props.FormatsLocale = reader.ReadString();
					changedList?.Add("FormatsLocale");
					break;
				case "InputSources":
					reader.ReadSignature("aa{ss}"u8);
					props.InputSources = ReadType_aaess(ref reader);
					changedList?.Add("InputSources");
					break;
				case "XSession":
					reader.ReadSignature("s"u8);
					props.XSession = reader.ReadString();
					changedList?.Add("XSession");
					break;
				case "Location":
					reader.ReadSignature("s"u8);
					props.Location = reader.ReadString();
					changedList?.Add("Location");
					break;
				case "LoginFrequency":
					reader.ReadSignature("t"u8);
					props.LoginFrequency = reader.ReadUInt64();
					changedList?.Add("LoginFrequency");
					break;
				case "LoginTime":
					reader.ReadSignature("x"u8);
					props.LoginTime = reader.ReadInt64();
					changedList?.Add("LoginTime");
					break;
				case "LoginHistory":
					reader.ReadSignature("a(xxa{sv})"u8);
					props.LoginHistory = ReadType_arxxaesvz(ref reader);
					changedList?.Add("LoginHistory");
					break;
				case "XHasMessages":
					reader.ReadSignature("b"u8);
					props.XHasMessages = reader.ReadBool();
					changedList?.Add("XHasMessages");
					break;
				case "XKeyboardLayouts":
					reader.ReadSignature("as"u8);
					props.XKeyboardLayouts = reader.ReadArrayOfString();
					changedList?.Add("XKeyboardLayouts");
					break;
				case "BackgroundFile":
					reader.ReadSignature("s"u8);
					props.BackgroundFile = reader.ReadString();
					changedList?.Add("BackgroundFile");
					break;
				case "IconFile":
					reader.ReadSignature("s"u8);
					props.IconFile = reader.ReadString();
					changedList?.Add("IconFile");
					break;
				case "Saved":
					reader.ReadSignature("b"u8);
					props.Saved = reader.ReadBool();
					changedList?.Add("Saved");
					break;
				case "Locked":
					reader.ReadSignature("b"u8);
					props.Locked = reader.ReadBool();
					changedList?.Add("Locked");
					break;
				case "PasswordMode":
					reader.ReadSignature("i"u8);
					props.PasswordMode = reader.ReadInt32();
					changedList?.Add("PasswordMode");
					break;
				case "PasswordHint":
					reader.ReadSignature("s"u8);
					props.PasswordHint = reader.ReadString();
					changedList?.Add("PasswordHint");
					break;
				case "AutomaticLogin":
					reader.ReadSignature("b"u8);
					props.AutomaticLogin = reader.ReadBool();
					changedList?.Add("AutomaticLogin");
					break;
				case "SystemAccount":
					reader.ReadSignature("b"u8);
					props.SystemAccount = reader.ReadBool();
					changedList?.Add("SystemAccount");
					break;
				case "LocalAccount":
					reader.ReadSignature("b"u8);
					props.LocalAccount = reader.ReadBool();
					changedList?.Add("LocalAccount");
					break;
				default:
					reader.ReadVariantValue();
					break;
			}
		}
		return props;
	}
}
record AccountsServiceProperties
{
	public string BackgroundFile { get; set; } = default!;
	public bool HasMessages { get; set; } = default!;
	public string[] KeyboardLayouts { get; set; } = default!;
}
partial class AccountsService : AccountsObject
{
	private const string __Interface = "org.freedesktop.DisplayManager.AccountsService";
	public AccountsService(AccountsDbusServiceFactory dbusServiceFactory, ObjectPath path) : base(dbusServiceFactory, path)
	{ }
	public Task SetBackgroundFileAsync(string value)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: "org.freedesktop.DBus.Properties",
				signature: "ssv",
				member: "Set");
			writer.WriteString(__Interface);
			writer.WriteString("BackgroundFile");
			writer.WriteSignature("s");
			writer.WriteString(value);
			return writer.CreateMessage();
		}
	}
	public Task SetHasMessagesAsync(bool value)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: "org.freedesktop.DBus.Properties",
				signature: "ssv",
				member: "Set");
			writer.WriteString(__Interface);
			writer.WriteString("HasMessages");
			writer.WriteSignature("b");
			writer.WriteBool(value);
			return writer.CreateMessage();
		}
	}
	public Task SetKeyboardLayoutsAsync(string[] value)
	{
		return this.Connection.CallMethodAsync(CreateMessage());
		MessageBuffer CreateMessage()
		{
			var writer = this.Connection.GetMessageWriter();
			writer.WriteMethodCallHeader(
				destination: DbusServiceFactory.Destination,
				path: Path,
				@interface: "org.freedesktop.DBus.Properties",
				signature: "ssv",
				member: "Set");
			writer.WriteString(__Interface);
			writer.WriteString("KeyboardLayouts");
			writer.WriteSignature("as");
			writer.WriteArray(value);
			return writer.CreateMessage();
		}
	}
	public Task<string> GetBackgroundFileAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "BackgroundFile"), (Message m, object? s) => ReadMessage_v_s(m, (AccountsObject)s!), this);
	public Task<bool> GetHasMessagesAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "HasMessages"), (Message m, object? s) => ReadMessage_v_b(m, (AccountsObject)s!), this);
	public Task<string[]> GetKeyboardLayoutsAsync()
		=> this.Connection.CallMethodAsync(CreateGetPropertyMessage(__Interface, "KeyboardLayouts"), (Message m, object? s) => ReadMessage_v_as(m, (AccountsObject)s!), this);
	public Task<AccountsServiceProperties> GetPropertiesAsync()
	{
		return this.Connection.CallMethodAsync(CreateGetAllPropertiesMessage(__Interface), (Message m, object? s) => ReadMessage(m, (AccountsObject)s!), this);
		static AccountsServiceProperties ReadMessage(Message message, AccountsObject _)
		{
			var reader = message.GetBodyReader();
			return ReadProperties(ref reader);
		}
	}
	public ValueTask<IDisposable> WatchPropertiesChangedAsync(Action<Exception?, PropertyChanges<AccountsServiceProperties>> handler, bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
	{
		return base.WatchPropertiesChangedAsync(__Interface, (Message m, object? s) => ReadMessage(m, (AccountsObject)s!), handler, emitOnCapturedContext, flags);
		static PropertyChanges<AccountsServiceProperties> ReadMessage(Message message, AccountsObject _)
		{
			var reader = message.GetBodyReader();
			reader.ReadString(); // interface
			List<string> changed = new(), invalidated = new();
			return new PropertyChanges<AccountsServiceProperties>(ReadProperties(ref reader, changed), ReadInvalidated(ref reader), changed.ToArray());
		}
		static string[] ReadInvalidated(ref Reader reader)
		{
			List<string>? invalidated = null;
			ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.String);
			while (reader.HasNext(arrayEnd))
			{
				invalidated ??= new();
				var property = reader.ReadString();
				switch (property)
				{
					case "BackgroundFile": invalidated.Add("BackgroundFile"); break;
					case "HasMessages": invalidated.Add("HasMessages"); break;
					case "KeyboardLayouts": invalidated.Add("KeyboardLayouts"); break;
				}
			}
			return invalidated?.ToArray() ?? Array.Empty<string>();
		}
	}
	private static AccountsServiceProperties ReadProperties(ref Reader reader, List<string>? changedList = null)
	{
		var props = new AccountsServiceProperties();
		ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.Struct);
		while (reader.HasNext(arrayEnd))
		{
			var property = reader.ReadString();
			switch (property)
			{
				case "BackgroundFile":
					reader.ReadSignature("s"u8);
					props.BackgroundFile = reader.ReadString();
					changedList?.Add("BackgroundFile");
					break;
				case "HasMessages":
					reader.ReadSignature("b"u8);
					props.HasMessages = reader.ReadBool();
					changedList?.Add("HasMessages");
					break;
				case "KeyboardLayouts":
					reader.ReadSignature("as"u8);
					props.KeyboardLayouts = reader.ReadArrayOfString();
					changedList?.Add("KeyboardLayouts");
					break;
				default:
					reader.ReadVariantValue();
					break;
			}
		}
		return props;
	}
}
partial class AccountsDbusServiceFactory
{
	public Tmds.DBus.Protocol.Connection Connection { get; }
	public string Destination { get; }
	public AccountsDbusServiceFactory(Tmds.DBus.Protocol.Connection connection, string destination)
		=> (Connection, Destination) = (connection, destination);
	public Accounts CreateAccounts(ObjectPath path) => new Accounts(this, path);
	public User CreateUser(ObjectPath path) => new User(this, path);
	public AccountsService CreateAccountsService(ObjectPath path) => new AccountsService(this, path);
}
class AccountsObject
{
	public AccountsDbusServiceFactory DbusServiceFactory { get; }
	public ObjectPath Path { get; }
	protected Tmds.DBus.Protocol.Connection Connection => DbusServiceFactory.Connection;
	protected AccountsObject(AccountsDbusServiceFactory dbusServiceFactory, ObjectPath path)
		=> (DbusServiceFactory, Path) = (dbusServiceFactory, path);
	protected MessageBuffer CreateGetPropertyMessage(string @interface, string property)
	{
		var writer = this.Connection.GetMessageWriter();
		writer.WriteMethodCallHeader(
			destination: DbusServiceFactory.Destination,
			path: Path,
			@interface: "org.freedesktop.DBus.Properties",
			signature: "ss",
			member: "Get");
		writer.WriteString(@interface);
		writer.WriteString(property);
		return writer.CreateMessage();
	}
	protected MessageBuffer CreateGetAllPropertiesMessage(string @interface)
	{
		var writer = this.Connection.GetMessageWriter();
		writer.WriteMethodCallHeader(
			destination: DbusServiceFactory.Destination,
			path: Path,
			@interface: "org.freedesktop.DBus.Properties",
			signature: "s",
			member: "GetAll");
		writer.WriteString(@interface);
		return writer.CreateMessage();
	}
	protected ValueTask<IDisposable> WatchPropertiesChangedAsync<TProperties>(string @interface, MessageValueReader<PropertyChanges<TProperties>> reader, Action<Exception?, PropertyChanges<TProperties>> handler, bool emitOnCapturedContext, ObserverFlags flags)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = DbusServiceFactory.Destination,
			Path = Path,
			Interface = "org.freedesktop.DBus.Properties",
			Member = "PropertiesChanged",
			Arg0 = @interface
		};
		return this.Connection.AddMatchAsync(rule, reader,
			(Exception? ex, PropertyChanges<TProperties> changes, object? rs, object? hs) => ((Action<Exception?, PropertyChanges<TProperties>>)hs!).Invoke(ex, changes),
			this, handler, emitOnCapturedContext, flags);
	}
	public ValueTask<IDisposable> WatchSignalAsync<TArg>(string sender, string @interface, ObjectPath path, string signal, MessageValueReader<TArg> reader, Action<Exception?, TArg> handler, bool emitOnCapturedContext, ObserverFlags flags)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = sender,
			Path = path,
			Member = signal,
			Interface = @interface
		};
		return this.Connection.AddMatchAsync(rule, reader,
			(Exception? ex, TArg arg, object? rs, object? hs) => ((Action<Exception?, TArg>)hs!).Invoke(ex, arg),
			this, handler, emitOnCapturedContext, flags);
	}
	public ValueTask<IDisposable> WatchSignalAsync(string sender, string @interface, ObjectPath path, string signal, Action<Exception?> handler, bool emitOnCapturedContext, ObserverFlags flags)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = sender,
			Path = path,
			Member = signal,
			Interface = @interface
		};
		return this.Connection.AddMatchAsync<object>(rule, (Message message, object? state) => null!,
			(Exception? ex, object v, object? rs, object? hs) => ((Action<Exception?>)hs!).Invoke(ex), this, handler, emitOnCapturedContext, flags);
	}
	protected static ObjectPath[] ReadMessage_ao(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		return reader.ReadArrayOfObjectPath();
	}
	protected static ObjectPath ReadMessage_o(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		return reader.ReadObjectPath();
	}
	protected static string[] ReadMessage_as(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		return reader.ReadArrayOfString();
	}
	protected static string ReadMessage_v_s(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		reader.ReadSignature("s"u8);
		return reader.ReadString();
	}
	protected static bool ReadMessage_v_b(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		reader.ReadSignature("b"u8);
		return reader.ReadBool();
	}
	protected static ObjectPath[] ReadMessage_v_ao(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		reader.ReadSignature("ao"u8);
		return reader.ReadArrayOfObjectPath();
	}
	protected static (long, long, long, long, long, long) ReadMessage_xxxxxx(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		var arg0 = reader.ReadInt64();
		var arg1 = reader.ReadInt64();
		var arg2 = reader.ReadInt64();
		var arg3 = reader.ReadInt64();
		var arg4 = reader.ReadInt64();
		var arg5 = reader.ReadInt64();
		return (arg0, arg1, arg2, arg3, arg4, arg5);
	}
	protected static ulong ReadMessage_v_t(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		reader.ReadSignature("t"u8);
		return reader.ReadUInt64();
	}
	protected static int ReadMessage_v_i(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		reader.ReadSignature("i"u8);
		return reader.ReadInt32();
	}
	protected static string[] ReadMessage_v_as(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		reader.ReadSignature("as"u8);
		return reader.ReadArrayOfString();
	}
	protected static Dictionary<string, string>[] ReadMessage_v_aaess(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		reader.ReadSignature("aa{ss}"u8);
		return ReadType_aaess(ref reader);
	}
	protected static long ReadMessage_v_x(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		reader.ReadSignature("x"u8);
		return reader.ReadInt64();
	}
	protected static (long, long, Dictionary<string, VariantValue>)[] ReadMessage_v_arxxaesvz(Message message, AccountsObject _)
	{
		var reader = message.GetBodyReader();
		reader.ReadSignature("a(xxa{sv})"u8);
		return ReadType_arxxaesvz(ref reader);
	}
	protected static Dictionary<string, string>[] ReadType_aaess(ref Reader reader)
	{
		List<Dictionary<string, string>> list = new();
		ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.Array);
		while (reader.HasNext(arrayEnd))
		{
			list.Add(ReadType_aess(ref reader));
		}
		return list.ToArray();
	}
	protected static Dictionary<string, string> ReadType_aess(ref Reader reader)
	{
		Dictionary<string, string> dictionary = new();
		ArrayEnd dictEnd = reader.ReadDictionaryStart();
		while (reader.HasNext(dictEnd))
		{
			var key = reader.ReadString();
			var value = reader.ReadString();
			dictionary[key] = value;
		}
		return dictionary;
	}
	protected static (long, long, Dictionary<string, VariantValue>)[] ReadType_arxxaesvz(ref Reader reader)
	{
		List<(long, long, Dictionary<string, VariantValue>)> list = new();
		ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.Struct);
		while (reader.HasNext(arrayEnd))
		{
			list.Add(ReadType_rxxaesvz(ref reader));
		}
		return list.ToArray();
	}
	protected static (long, long, Dictionary<string, VariantValue>) ReadType_rxxaesvz(ref Reader reader)
	{
		return (reader.ReadInt64(), reader.ReadInt64(), reader.ReadDictionaryOfStringToVariantValue());
	}
	protected static void WriteType_aaess(ref MessageWriter writer, Dictionary<string, string>[] value)
	{
		ArrayStart arrayStart = writer.WriteArrayStart(DBusType.Array);
		foreach (var item in value)
		{
			WriteType_aess(ref writer, item);
		}
		writer.WriteArrayEnd(arrayStart);
	}
	protected static void WriteType_aess(ref MessageWriter writer, Dictionary<string, string> value)
	{
		ArrayStart arrayStart = writer.WriteDictionaryStart();
		foreach (var item in value)
		{
			writer.WriteDictionaryEntryStart();
			writer.WriteString(item.Key);
			writer.WriteString(item.Value);
		}
		writer.WriteDictionaryEnd(arrayStart);
	}
}
class PropertyChanges<TProperties>
{
	public PropertyChanges(TProperties properties, string[] invalidated, string[] changed)
		=> (Properties, Invalidated, Changed) = (properties, invalidated, changed);
	public TProperties Properties { get; }
	public string[] Invalidated { get; }
	public string[] Changed { get; }
	public bool HasChanged(string property) => Array.IndexOf(Changed, property) != -1;
	public bool IsInvalidated(string property) => Array.IndexOf(Invalidated, property) != -1;
}
