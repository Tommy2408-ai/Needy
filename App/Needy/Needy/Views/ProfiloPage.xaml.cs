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
			await CaricaDatiProfilo();
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
				}
            }
			catch (Exception ex)
			{
				await DisplayAlert("Errore", "Impossibile caricare il profilo.", "OK");
			}
		}
	}
}