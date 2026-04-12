using Microsoft.Maui.Controls;
using Needy.Models;
using pocketbase_csharp_sdk;
using System;
using System.Transactions;

namespace Needy.Views
{
	public partial class NuovaRichiestaPage : ContentPage
	{
		private readonly PocketBase _pb;

		public NuovaRichiestaPage(PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
		}

		private async void OnPubblicaClicked(object sender, EventArgs e)
		{
			// 1. Controlliamo che non invii una richiesta vuota
			if (string.IsNullOrWhiteSpace(TitoloEntry.Text) || string.IsNullOrWhiteSpace(DescrizioneEditor.Text))
			{
				StatusLabel.Text = "Per favore, compila Titolo e Descrizione!";
				StatusLabel.TextColor = Colors.Red;
				StatusLabel.IsVisible = true;
				return; // Ferma tutto
			}

			PubblicaButton.IsEnabled = false;
			PubblicaButton.Text = "INVIO IN CORSO...";
			StatusLabel.IsVisible = false;

			try
			{
				// 1. Controllo di sicurezza: se la sessione è vuota, blocca tutto
				if (_pb.AuthStore.Model == null)
				{
					await DisplayAlert("Errore", "Non sei autenticato. Fai di nuovo il login.", "OK");
					return;
				}

				string mioId = _pb.AuthStore.Model.Id;

				// 2. Preara la durata
				double durata = 1;
				if (!string.IsNullOrWhiteSpace(DurataEntry.Text))
				{
					double.TryParse(DurataEntry.Text.Replace(".", ","), out durata);
				}

				var nuovaRichiesta = new Richiesta
				{
					title = TitoloEntry.Text,
					description = DescrizioneEditor.Text,
					// ATTENZIONE: La categoria deve essere identica a quella su PocketBase.
					category = CategoriaPicker.SelectedItem?.ToString() ?? "Casa",
					estimated_duration = durata,
					status = "APERTA",
					requester = mioId,
					assistant = null
				};

				// 3. Inviamo 
				var risultato = await _pb.Records.CreateAsync<Richiesta>("requests", nuovaRichiesta);

				// 4. Verifichiamo il risultato
				if (risultato.IsSuccess) // Se il risultato non è nullo, ha creato il record!
				{
					await DisplayAlert("Fantastico!", "La tua richiesta di aiuto è stata pubblicata.", "OK");
					await Navigation.PopAsync();	
				}
				else
				{
					string errore = risultato.Errors.Count > 0 ? risultato.Errors[0].Message : "Errore sconosciuto";
					await DisplayAlert("PocketBase ha rifiutato!", errore, "OK");

					StatusLabel.Text = "Errore durante l'invio. Riprova.";
					StatusLabel.TextColor = Colors.Red;
					StatusLabel.IsVisible = true;
				}
			}
			catch (Exception ex)
			{
				// ORA STAMPIAMO IL VERO ERRORE DI POCKETBASE A SCHERMO!
				// Così capiamo subito se si lamenta di qualche altro campo.
				await DisplayAlert("Dettaglio Errore", ex.Message, "OK");

				StatusLabel.Text = "Errore durante l'invio. Riprova.'";
				StatusLabel.TextColor = Colors.Red;
				StatusLabel.IsVisible = true;
			}
			finally
			{
				PubblicaButton.IsEnabled = true;
				PubblicaButton.Text = "PUBBLICA RICHIESTA";
			}
		}
	}
}