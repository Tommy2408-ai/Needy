using Needy.Models;
using pocketbase_csharp_sdk;

namespace Needy.Views
{
	public partial class SceltaCandidatiPage : ContentPage
	{
		private readonly PocketBase _pb;
		private Richiesta _richiestaAttuale;
		public SceltaCandidatiPage(Richiesta richiesta, PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
			_richiestaAttuale = richiesta;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await CaricaCandidati();
		}

		private async Task CaricaCandidati()
		{
			try
			{
				var listaCandidati = new List<CandidatoInfo>();

				if (_richiestaAttuale.candidates != null)
				{
					foreach (string idCandidato in _richiestaAttuale.candidates)
					{
						var risposta = await _pb.Records.GetOneAsync<User>("users", idCandidato);
						if (risposta.IsSuccess && risposta.Value != null)
						{
							string messaggioScritto = "Nessun messaggio.";
							if (_richiestaAttuale.candidate_messages != null && _richiestaAttuale.candidate_messages.ContainsKey(idCandidato))
								messaggioScritto = _richiestaAttuale.candidate_messages[idCandidato];

							listaCandidati.Add(new CandidatoInfo
							{
								Utente = risposta.Value,
								Messaggio = messaggioScritto
							});
						}
					}
				}

				CandidatiCollection.ItemsSource = listaCandidati;
			}
			catch (Exception ex)
			{
				await DisplayAlert("Errore", "Impossibile caricare i candidati.", "OK");
			}
		}

		private async void OnScegliCandidatoClicked(object sender, EventArgs e)
		{
			var bottoneCliccato = (Button)sender;

			var infoSelezionata = bottoneCliccato.BindingContext as CandidatoInfo;
			if (infoSelezionata == null) return;

			var utenteScelto = infoSelezionata.Utente;

			bool conferma = await DisplayAlert("Conferma", $"Vuoi affidare la richiesta a {utenteScelto.Name}?", "SI", "NO");

			if (conferma)
			{
				bottoneCliccato.IsEnabled = false;
				bottoneCliccato.Text = "ATTENDI...";

				try
				{
					_richiestaAttuale.assistant = utenteScelto.Id;
					_richiestaAttuale.status = "IN_CORSO";

					var risultato = await _pb.Records.UpdateAsync<Richiesta>("requests", _richiestaAttuale);

					if (risultato.IsSuccess)
					{
						await DisplayAlert("Fatto!", $"Hai assegnato la richiesta a {utenteScelto.Name}. Ora la richiesta è IN CORSO.", "OK");

						await Navigation.PopToRootAsync();
					}
					else
					{
						string errore = risultato.Errors.Count > 0 ? risultato.Errors[0].Message : "Errore sconosciuto";
						await DisplayAlert("Errore PocketBase", errore, "OK");
					}
				}
				catch (Exception ex)
				{
					await DisplayAlert("Errore", ex.Message, "OK");
				}
				finally
				{
					bottoneCliccato.IsEnabled = true;
					bottoneCliccato.Text = "ACCETTA";
				}
			}
		}
	}
}