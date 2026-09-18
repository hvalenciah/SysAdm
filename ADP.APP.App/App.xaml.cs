using System.Data;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace SysAdm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Configuración de la aplicación cargada desde appsettings.json.
        /// </summary>
        public static IConfiguration Configuration { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .Build();
        }
    }
}
