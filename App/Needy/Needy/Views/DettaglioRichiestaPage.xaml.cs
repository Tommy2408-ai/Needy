using Microsoft.Maui.Controls;
using Needy.Models;
using pocketbase_csharp_sdk;
using System;

namespace Needy.Views
{
	public partial class DettaglioRichiestaPage : ContentPage
	{
		private readonly PocketBase _pb;
		private Richiesta _richiestaAttuale;

		public DettaglioRichiestaPage(Richiesta richiesta, PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
			_richiestaAttuale = richiesta;

			this.BindingContext = _richiestaAttuale;
		}

		private async void OnAccettaClicked(object sender, EventArgs e)
		{
			string mioId = await SecureStorage.Default.GetAsync("mio_id");

			if (string.IsNullOrEmpty(mioId))
			{
				await DisplayAlert("Errore", "Sessione scaduta. Devi fare il login per accettare.", "OK");
				return;
			}

			// Controllo logico: Non puoi accettare la TUA STESSA richiesta
			if (_richiestaAttuale.requester == mioId)
			{
				await DisplayAlert("Ehi!", "Non puoi offrirti volontario per una richiesta creata da te stesso.", "OK");
				return;
			}

			// Verifica che l'oggetto abbia un ID valido
			if (string.IsNullOrEmpty(_richiestaAttuale.Id))
			{
				await DisplayAlert("Errore", "ID richiesta non valido. Ricarica la pagina.", "OK");
				return;
			}

			AccettaButton.IsEnabled = false; // Disabilita il pulsante per evitare clic multipli
			AccettaButton.Text = "ACCETTAZIONE IN CORSO...";

			try
			{
				// Aggiorna i campi dell'oggetto
				_richiestaAttuale.status = "IN_CORSO";
				_richiestaAttuale.assistant = mioId;

				// UpdateAsync richiede l'oggetto con tutti i dati (compreso l'ID)
				var risultato = await _pb.Records.UpdateAsync<Richiesta>("requests", _richiestaAttuale);

				if (risultato.IsSuccess)
				{
					await DisplayAlert("Grazie di cuore! ❤️", "Hai accettato  la richiesta. Adesso la richiesta risulta IN CORSO.", "OK");

					await Navigation.PopAsync();
				}
				else
				{
					string errore = risultato.Errors.Count > 0 ? risultato.Errors[0].Message : "Errore sconosciuto";
					await DisplayAlert("Impossibile accettare", errore, "OK");

					_richiestaAttuale.status = "Aperta";
					_richiestaAttuale.assistant = null;
				}
			}
			catch (Exception ex)
			{
				// Log completo dell'eccezione per il debugging
				string errorMessage = $"Errore: {ex.GetType().Name}\n\n{ex.Message}";
				if (ex.InnerException != null)
				{
					errorMessage += $"\n\nInner Exception: {ex.InnerException.Message}";
				}

				await DisplayAlert("Errore di Rete", errorMessage, "OK");

				_richiestaAttuale.status = "Aperta";
				_richiestaAttuale.assistant = null;
			}
			finally
			{
				AccettaButton.IsEnabled = true; // Riabilita il pulsante
				AccettaButton.Text = "MI OFFRO IO";
			}
		}
    }
}