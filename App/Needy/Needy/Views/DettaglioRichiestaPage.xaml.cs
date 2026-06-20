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

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			string mioId = await SecureStorage.Default.GetAsync("mio_id");

			if (_richiestaAttuale.candidates != null && _richiestaAttuale.candidates.Contains(mioId))
			{
				AccettaButton.Text = "GIÀ CANDIDATO";
				AccettaButton.BackgroundColor = new Color(216, 138, 74);
            }
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
				// Controlliamo se si è già candidato
				if (_richiestaAttuale.candidates != null && _richiestaAttuale.candidates.Contains(mioId))
				{
					await DisplayAlert("Ehi!", "Ti sei già candidato per questa richiesta. Attendi la risposta di chi l'ha creata.", "OK");
					return;
				}

				if (_richiestaAttuale.candidates == null)
				{
					_richiestaAttuale.candidates = new List<string>();
				}

				_richiestaAttuale.candidates.Add(mioId);

				// Nota: NON cambiamo lo stato. La richiesta rimane "Aperta" così altri possono offrirsi.
				// Nota: NON riempiamo "assistant". Quello lo riempirà il requester dopo.

				var risultato = await _pb.Records.UpdateAsync<Richiesta>("requests", _richiestaAttuale);

				if (risultato.IsSuccess)
				{
					await DisplayAlert("Candidatura inviata! 🙋‍♂️", "Ti sei candidato per questa richiesta. L'autore riceverà una notifica e potrà sceglierti.", "OK");
                    AccettaButton.Text = "GIÀ CANDIDATO";
                    AccettaButton.BackgroundColor = new Color(216, 138, 74);
                    await Navigation.PopAsync();
				}
				else
				{
					string errore = risultato.Errors.Count > 0 ? risultato.Errors[0].Message : "Errore sconosciuto";
					await DisplayAlert("Impossibile candidarsi", errore, "OK");

					_richiestaAttuale.candidates.Remove(mioId);
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
				
			}
		}
    }
}