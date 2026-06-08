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
			await DisplayAlert("In lavorazione", "A breve potrai accettare questa richiesta!", "OK");
		}
	}
}