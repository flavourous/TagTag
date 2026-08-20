using Microsoft.Extensions.DependencyInjection;
using TagTag.Backend;
using TagTag.ViewModels;

namespace TagTag;

public static class CompositionRoot
{
    private static readonly ServiceCollection Services = new();
    private static bool platformRegistered;

    public static void PlatformRegister(IPlatform platform)
    {
        if (platformRegistered) return;
        platformRegistered = true;
        Services.AddSingleton(platform);
        Services.AddSingleton<MainViewModel>();
    }

    public static IServiceProvider Build()
    {
        if (!platformRegistered) throw new InvalidOperationException("Register a platform before building the app.");
        return Services.BuildServiceProvider();
    }
}
