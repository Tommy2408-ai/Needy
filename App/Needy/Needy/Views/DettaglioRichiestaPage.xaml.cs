using Microsoft.Maui.Controls;
using Needy.Models;
using pocketbase_csharp_sdk;
using System;
using System.Diagnostics;

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

			LoadingOverlay.IsVisible = true;

			try
			{
				var autore = await _pb.Records.GetOneAsync<User>("users", _richiestaAttuale.requester);

				if (autore.IsSuccess && autore.Value != null)
				{
					AutoreCard.BindingContext = autore.Value;
					AutoreCard.IsVisible = true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Errore caricamento autore: {ex.Message}");
			}

			string mioId = await SecureStorage.Default.GetAsync("mio_id");

			if (_richiestaAttuale.status == "COMPLETATA")
				CompletataLabel.IsVisible = true;
			else if (_richiestaAttuale.requester == mioId)
			{
				if (_richiestaAttuale.status == "IN_CORSO")
					CompletaButton.IsVisible = true;
			}
			else
			{
				if (_richiestaAttuale.status == "APERTA")
				{
					AccettaButton.IsVisible = true;

					if (_richiestaAttuale.candidates != null && _richiestaAttuale.candidates.Contains(mioId))
					{
						AccettaButton.Text = "GIÀ CANDIDATO";
						AccettaButton.BackgroundColor = new Color(216, 138, 74);
						AccettaButton.IsEnabled = false;

                    }
				}
			}

            LoadingOverlay.IsVisible = false;
        }

		private async void OnAutoreTapped(object sender, EventArgs e)
		{
			var card = (Frame)sender;
			var utenteCliccato = card.BindingContext as User;

			if (utenteCliccato != null)
			{
				await card.FadeTo(0.5, 100);
				await card.FadeTo(1, 100);

				await DisplayAlert("Profilo", $"Vuoi vedere il profilo di {utenteCliccato.Name}?\nHa donato {utenteCliccato.reputation_hour} ore!", "OK");
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

				string messaggio = await DisplayPromptAsync(
					title: "Ti stai offrendo!",
					message: "Scrivi un breve messaggio all'autore (opzionale):",
					accept: "INVIA",
					cancel: "ANNULLA",
					placeholder: "Es: Ciao, posso passare verso le 15:00");

				if (messaggio == null)
				{
					AccettaButton.IsEnabled = true;
					AccettaButton.Text = "MI OFFRO IO";
					return;
				}

				AccettaButton.IsEnabled = false;
				AccettaButton.Text = "CANDIDATURA IN CORSO...";

				if (_richiestaAttuale.candidates == null)
				{
					_richiestaAttuale.candidates = new List<string>();
				}

				_richiestaAttuale.candidates.Add(mioId);

				if (_richiestaAttuale.candidate_messages == null)
					_richiestaAttuale.candidate_messages = new Dictionary<string, string>();

				_richiestaAttuale.candidate_messages[mioId] = messaggio;

				// Nota: NON cambiamo lo stato. La richiesta rimane "Aperta" così altri possono offrirsi.
				// Nota: NON riempiamo "assistant". Quello lo riempirà il requester dopo.

				var risultato = await _pb.Records.UpdateAsync<Richiesta>("requests", _richiestaAttuale);

				if (risultato.IsSuccess)
				{
					await DisplayAlert("Candidatura inviata! 🙋‍♂️", "Ti sei candidato per questa richiesta. L'autore riceverà una notifica e potrà sceglierti.", "OK");
                    AccettaButton.Text = "GIÀ CANDIDATO";
                    AccettaButton.BackgroundColor = new Color(216, 138, 74);
                }
				else
				{
					string errore = risultato.Errors.Count > 0 ? risultato.Errors[0].Message : "Errore sconosciuto";
					await DisplayAlert("Impossibile candidarsi", errore, "OK");

					_richiestaAttuale.candidates.Remove(mioId);
					_richiestaAttuale.candidate_messages.Remove(mioId);
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

		private async void OnCompletaClicked(object sender, EventArgs e)
		{
			bool conferma = await DisplayAlert("Conferma", "Vuoi chiudere questa richiesta? Le ore verranno assegnate all'aiutante.", "SÌ", "NO");
			if (!conferma) return;

			CompletaButton.IsEnabled = false;
			CompletaButton.Text = "CARICAMENTO";

			try
			{
				_richiestaAttuale.status = "COMPLETATA";
				await _pb.Records.UpdateAsync<Richiesta>("requests", _richiestaAttuale);

				if (!string.IsNullOrEmpty(_richiestaAttuale.assistant))
				{
					var aiutante = await _pb.Records.GetOneAsync<User>("users", _richiestaAttuale.assistant);

					if (aiutante.IsSuccess && aiutante.Value != null)
					{
						aiutante.Value.Id = _richiestaAttuale.assistant;

						aiutante.Value.reputation_hour += (int)Math.Ceiling(_richiestaAttuale.estimated_duration);

						var updateUtente = await _pb.Records.UpdateAsync<User>("users", aiutante.Value);

						if (!updateUtente.IsSuccess)
						{
							string err = updateUtente.Errors.Count > 0 ? updateUtente.Errors[0].Message : "Errore ignoto";
							await DisplayAlert("Errore Ore", $"Impossibile aggiornare le ore: {err}", "OK");
						}
					}
				}

				await DisplayAlert("Fatto!", "Richiesta completata e ore assegnate! Grazie per aver usato Needy.", "OK");
				await Navigation.PopAsync();
			}
			catch (Exception ex)
			{
				await DisplayAlert("Errore", "Impossibile completare la richiesta.", "OK");
				_richiestaAttuale.status = "IN_CORSO";
				CompletaButton.IsEnabled = true;
				CompletaButton.Text = "✓ CONTRASSEGNA COME COMPLETATA";

            }
		}
    }
}