using pocketbase_csharp_sdk;
using Needy.Models;

namespace Needy.Views
{
	public partial class NotificationsPage : ContentPage
	{
		private readonly PocketBase _pb;

		public NotificationsPage(PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await LoadNotifications();
		}

		private async Task LoadNotifications()
		{
			try
			{
				string myId = await SecureStorage.Default.GetAsync("my_id");
				if (string.IsNullOrEmpty(myId)) return;

				var response = await _pb.Records.GetFullListAsync<Request>("requests");

				if (response.IsSuccess && response.Value != null)
				{
                    // FILTER:
                    // Am I the creator?
                    // The candidates are not null and there is at least 1 person?
                    // Is the request still "Open"?
                    var unreadNotifications = response.Value.Where(r => r.requester == myId && r.candidates != null && r.candidates.Count > 0 && r.status == "OPEN").ToList();

					NotificationsCollection.ItemsSource = unreadNotifications;
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", "Unable to load notifications.", "OK");
			}
		}

		private async void OnViewCandidatesClicked(object sender, EventArgs e)
		{
			var clickedButton = (Button)sender;
			var selectedRequest = clickedButton.BindingContext as Request;

			if (selectedRequest != null)
			{
				await Navigation.PushAsync(new CandidatesSelectionPage(selectedRequest, _pb));
			}
		}
	}
}