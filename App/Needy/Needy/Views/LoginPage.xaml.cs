using FluentResults;
using Microsoft.Maui.Controls;
using Needy.Models;
using pocketbase_csharp_sdk;
using pocketbase_csharp_sdk.Helper.Convert;
using pocketbase_csharp_sdk.Models;
using pocketbase_csharp_sdk.Models.Auth;
using pocketbase_csharp_sdk.Services;
using System;

namespace Needy.Views
{
    public partial class LoginPage : ContentPage
    {
        // Variable for PocketBase that contains the connection
        private readonly PocketBase _pb;

        // Quando MAUI apre questa pagina, vede che ha bisogno di un "PocketBase".
        // Va a prenderlo da MauiPrograms.cs e ce lo "inietta" qui dentro tramite la variabile (pb).
        public LoginPage(PocketBase pb)
        {
            InitializeComponent();

            // Salviamo la connessione globale nella nostra variabile privata
            _pb = pb;
        }

        // Questo è l'evento che parte quando clicchi "ENTRA"
        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // Nascondiamo vecchi errori e blocchiamo il bottone
            ErrorLabel.IsVisible = false;
            LoginButton.IsEnabled = false;
            LoginButton.Text = "ACCESSO IN CORSO...";

            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            try
            {
                // AZIONE 2: LA CHIAMATA
                // Usiamo <Needy.Models.User> per dire a PocketBase di trasformare i dati
                // direttamente nella classe
                // NOTA: Se ti dà errore rosso su <Needy.Models.User>, toglilo, ma di solito questa libreria lo supporta.
                var authData = await _pb.User.AuthenticateWithPasswordAsync(email, password);

                // AZIONE 3: IL SEMAFORO
                // Adesso che i dati sono arrivati, dobbiamo estrarre l'utente.
                // authData di solito contiene il "Token" e il "User" o "Record".
                // Scrivi "authData." su Visual Studio e guarda cosa ti suggerisce! Di solito è authData.User o authData.Record.

                // (Assumiamo che la proprietà si chiami User, modificala se Visual Studio ti suggerisce Record)
                var utenteLoggato = authData.Value;

                // Ora controllare is_verified è facilissimo perché è nella nostra classe!
                if (utenteLoggato != null && utenteLoggato.Record.Verified == true)
                {
                    // VERDE! L'Admin lo ha accettato.
                    await DisplayAlert("Benvenuto", $"Accesso eseguito con successo, {utenteLoggato.Record.Username}!", "OK");

                    // TODO: Salvare il Token in SecureStorage e andare alla Home
                }
                else
                {
                    // GIALLO! Password giusta, ma Admin non ha ancora accettato (isVerified = false)
                    _pb.AuthStore.Clear(); // Lo disconnettiamo subito per sicurezza del lato app

                    ErrorLabel.Text = "Il tuo account è in attesa di approvazione da un amministratore.";
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                // ROSSO! Qualcosa è andato storto (Email errata, Password errata, o Server spento)
                // Se vuoi vedere l'errore tecnico per te, puoi scommentare la riga sotto:
                // await DisplayAlert("Errore di Sistema", ex.Message, "OK");

                ErrorLabel.Text = "Email o Password errati, oppure server non raggiungibile.";
                ErrorLabel.IsVisible = true;
            }
            finally
            {
                // Questo blocco viene eseguito SEMPRE alla fine, sia in caso di successo che di errore
                // Riaccendiamo il bottone
                LoginButton.IsEnabled = true;
                LoginButton.Text = "ENTRA";
            }
        }
    }
}