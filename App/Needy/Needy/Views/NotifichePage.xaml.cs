using pocketbase_csharp_sdk;
using Needy.Models;

namespace Needy.Views
{
	public partial class NotifichePage : ContentPage
	{
		private readonly PocketBase _pb;

		public NotifichePage(PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await CaricaNotifiche();
		}

		private async Task CaricaNotifiche()
		{
			try
			{
				string mioId = await SecureStorage.Default.GetAsync("mio_id");
				if (string.IsNullOrEmpty(mioId)) return;

				var response = await _pb.Records.GetFullListAsync<Richiesta>("requests");

				if (response.IsSuccess && response.Value != null)
				{
					// FILTRO:
					// Sono io il creatore?
					// I candati non sono null e c'è almeno 1 persona?
					// La richiesta è ancora "Aperta"?
					var notificheDaVedere = response.Value.Where(r => r.requester == mioId && r.candidates != null && r.candidates.Count > 0 && r.status == "APERTA").ToList();

					NotificheCollection.ItemsSource = notificheDaVedere;
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Errore", "Impossibile caricare le notifiche.", "OK");
			}
		}

		private async void OnVediCandidatiClicked(object sender, EventArgs e)
		{
			var bottoneCliccato = (Button)sender;
			var richiestaSelezionata = bottoneCliccato.BindingContext as Richiesta;

			if (richiestaSelezionata != null)
			{
				await DisplayAlert("Presto disponibile", $"A breve potrai scegliere chi accetterà '{richiestaSelezionata.title}'", "OK");
			}
		}
	}
}