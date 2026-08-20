using TagTag.Backend;

namespace TagTag;

public sealed class Platform : IPlatform
{
    public Platform(string appData)
    {
        AppData = appData;
        Directory.CreateDirectory(appData);
    }

    public int AppVersion => 1;
    public string AppData { get; }
    public void WriteLine(string message) => Console.WriteLine(message);
    public void DeleteFile(string path) { if (File.Exists(path)) File.Delete(path); }
    public Stream ReadFile(string path) => File.OpenRead(path);
}
