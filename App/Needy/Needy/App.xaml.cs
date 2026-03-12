using Microsoft.Extensions.DependencyInjection;
using Needy.Views;
using pocketbase_csharp_sdk;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics; // Serve per il MainThread

namespace Needy
{
    public partial class App : Application
    {
        private readonly PocketBase _pb;

        // Modifichiamo il costruttore per ricevere la connessione a PocketBase
        public App(PocketBase pb)
        {
            InitializeComponent();
            _pb = pb;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Creiamo la "Finestra" dell'app mettendoci dentro il Login come base
            var window = new Window(new LoginPage(_pb));

            // Lanciamo il controllo della cassaforte passando la finestra
            CheckToken(window);

            return window;
        }

        private async void CheckToken(Window window)
        {
            try
            {
                // 1. Apriamo la cassaforte in background
                string savedToken = await SecureStorage.Default.GetAsync("auth_token");

                // 2. Il token esiste?
                if (!string.IsNullOrEmpty(savedToken))
                {
                    // Diciamo alla libreria di PocketBase di ricordarsi questo token
                    _pb.AuthStore.Save(savedToken, null);

                    // TRUCCO MAGICO: Diciamo a Window "Ehi, torna sul flusso principale (UI Thread) e cambia la pagina!"
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Sostituiamo la pagina dentro la finestra
                        window.Page = new HomePage(_pb);
                    });
                }
            }
            catch (Exception ex)
            {
                // Se la cassaforte è rotta o vuota, ignoriamo l'errore e restiamo sul Login
                Debug.WriteLine($"Errore SecureStorage: {ex.Message}");
            }
        }
    }
}