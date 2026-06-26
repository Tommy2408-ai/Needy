using Microsoft.Extensions.Logging;
using Needy.Views; 
using pocketbase_csharp_sdk;

namespace Needy
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Diciamo all'app che esiste la pagina di Login
            builder.Services.AddTransient<LoginPage>();

            // AGGIUNGI QUESTA RIGA: Diciamo all'app che esiste anche la HomePage
            builder.Services.AddTransient<HomePage>();

            builder.Services.AddTransient<NuovaRichiestaPage>();

            builder.Services.AddTransient<ProfiloPage>();

            builder.Services.AddTransient<NotifichePage>();
            
            builder.Services.AddTransient<SceltaCandidatiPage>();

            builder.Services.AddTransient<RegistrazionePage>();
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // ---------------------------------------------------------
            // 🔌 POCKETBASE CONFIGURATION
            // ---------------------------------------------------------

            // 1. Define the server address.
            // NOTE: If you use Windows it's localhost (127.0.0.1)
            // If you use the Android emulator, localhost doesn't work; use 10.0.2.2.
            //string url = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:8090" : "http://127.0.0.1:8090";
            // If you use an external device, use the IP of the PC
            string url = "http://192.168.1.166:8090";

            // 2. Register PocketBase as a singleton.
            // Means: "Create this connection NOW and keep it alive for the lifetime of the app".
            builder.Services.AddSingleton(new PocketBase(url));


            // ---------------------------------------------------------
            // 📱 PAGE REGISTRATION (Used for Dependency Injection)
            // ---------------------------------------------------------
            // Tell the app that the Login page exists
            builder.Services.AddTransient<LoginPage>();


            return builder.Build();
        }
    }
}
