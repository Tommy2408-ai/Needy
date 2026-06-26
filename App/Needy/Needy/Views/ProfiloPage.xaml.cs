using Needy.Models;
using pocketbase_csharp_sdk;

namespace Needy.Views
{
	public partial class ProfiloPage : ContentPage
	{
		private readonly PocketBase _pb;

		public ProfiloPage(PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			LoadingOverlay.IsVisible = true;
			await CaricaDatiProfilo();
			LoadingOverlay.IsVisible = false;
        }

		private async Task CaricaDatiProfilo()
		{
			try
			{
				string mioId = await SecureStorage.Default.GetAsync("mio_id");

				if (!string.IsNullOrEmpty(mioId))
				{
					var mioProfilo = await _pb.Records.GetOneAsync<User>("users", mioId);

					if (mioProfilo.IsSuccess && mioProfilo.Value != null)
					{
                        this.BindingContext = mioProfilo.Value;
                    }

					var tutteLeRichieste = await _pb.Records.GetFullListAsync<Richiesta>("requests");

					if (tutteLeRichieste.IsSuccess && tutteLeRichieste.Value != null)
					{
						var ilMioStorico = new List<Richiesta>();

						foreach (var r in tutteLeRichieste.Value)
						{
							// Created by me
							if (r.requester == mioId)
							{
								r.MyRule = "Created by me";
								ilMioStorico.Add(r);
							}
							// I'm the helper
							else if (r.assistant == mioId)
							{
								r.MyRule = "You're helping";
								ilMioStorico.Add(r);
							}
							// I'm a candidate, waiting for response
							else if (r.candidates != null && r.candidates.Contains(mioId))
							{
								r.MyRule = "Waiting for response";
								ilMioStorico.Add(r);
							}
						}

						StoricoCollection.ItemsSource = ilMioStorico;
					}
				}
            }
			catch (Exception ex)
			{
				await DisplayAlert("Errore", "Impossibile caricare il profilo.", "OK");
			}
		}
	}
}