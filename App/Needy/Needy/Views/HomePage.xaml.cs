using Microsoft.Maui.Controls;
using Needy.Models;
using pocketbase_csharp_sdk;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Needy.Views
{
	public partial class HomePage : ContentPage
	{
		private readonly PocketBase _pb;

		public HomePage(PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
		}

		// QUESTO EVENTO PARTE OGNI VOLTA CHE LA PAGINA VIENE MOSTRATA A SCHERMO
		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await CaricaIntestazione();
			await CaricaRichieste();
		}

		private async Task CaricaIntestazione()
		{
			try
			{
				string mioId = await SecureStorage.Default.GetAsync("mio_id");

				if (!string.IsNullOrEmpty(mioId))
				{
					var utente = await _pb.Records.GetOneAsync<User>("users", mioId);

					if (utente.IsSuccess && utente.Value != null)
					{
						HeaderView.BindingContext = utente.Value;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Errore caricamento intestazione: " + ex.Message);
			}
		}

		private async Task CaricaRichieste()
		{
			try
			{
				// 1. Chiediamo a PocketBase l'intera lista della collezione "requests"
				// E la convertiamo automaticamente nella nostra classe <Richiesta>
				var listaRichieste = await _pb.Records.GetFullListAsync<Richiesta>("requests");

				// 2. Diciamo alla nostra grafica (RichiesteCollection) di usare questi dati!
				RichiesteCollection.ItemsSource = listaRichieste.Value;
			}
			catch (Exception ex)
			{
				// Se c'è un errore (es. server spento) lo scriviamo
				await DisplayAlert("Errore", "Impossibile caricare la bacheca: " + ex.Message, "OK");
			}
		}

		private void OnLogoutClicked(object sender, EventArgs e)
		{
			// 1. Dichiariamo a PocketBase di scordarsi di noi
			_pb.AuthStore.Clear();

			// 2. Cancelliamo il token dalla cassaforte del telefono
			SecureStorage.Default.Remove("auth_token");

			// 3. Rimandiamo l'utente alla pagina di Login
			Application.Current.MainPage = new NavigationPage(new LoginPage(_pb));
		}

		private async void OnNuovaRichiestaClicked(object sender, EventArgs e)
		{
			// Assicuriamo che Navigation sia disponibile
			if (Navigation != null && Navigation.NavigationStack.Count > 0)
			{
				await Navigation.PushAsync(new NuovaRichiestaPage(_pb));
			}
			else
			{
				// Fallback: Sostituisci il MainPage se Navigation non è disponibile
				Application.Current.MainPage = new NavigationPage(new NuovaRichiestaPage(_pb));
			}
		}

		private async void OnRichiestaTapped(object sender, TappedEventArgs e)
		{
			// 1. Capiamo quale "Card" (Frame) è stata toccata dal dito dell'utente
			var frameCliccato = (Frame)sender;

			// 2. Estraiamo i dati (la Richiesta) che MAUI aveva collegato a questa specifica Card
			var richiestaSelezionata = frameCliccato.BindingContext as Richiesta;

			// 3. Se abbiamo i dati, partiamo per i Dettagli
			if (richiestaSelezionata != null)
			{
				await frameCliccato.FadeTo(0.5, 100);
				await frameCliccato.FadeTo(1, 100);

				await Navigation.PushAsync(new DettaglioRichiestaPage(richiestaSelezionata, _pb));
			}
		}

		private async void OnProfiloClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new ProfiloPage(_pb));
		}
	}
}