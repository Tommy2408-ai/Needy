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

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			LoadingOverlay.IsVisible = true;

			await LoadHeader();
			await LoadRequests();

			LoadingOverlay.IsVisible = false;
		}

		private async Task LoadHeader()
		{
			try
			{
				string myId = await SecureStorage.Default.GetAsync("my_id");

				if (!string.IsNullOrEmpty(myId))
				{
					 var user = await _pb.Records.GetOneAsync<User>("users", myId);

					if (user.IsSuccess && user.Value != null)
					{
						HeaderView.BindingContext = user.Value;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error loading header: " + ex.Message);
			}
		}

		private async Task LoadRequests()
		{
			try
			{
				string myId = await SecureStorage.Default.GetAsync("my_id");

                // 1. We ask PocketBase for the entire list of the "requests" collection
                // And we automatically convert it to our <Request> class
                var requestsList = await _pb.Records.GetFullListAsync<Request>("requests");

				if (requestsList.IsSuccess && requestsList.Value != null)
				{
					var filteredBoard = requestsList.Value.Where(r => r.requester != myId && r.status == "OPEN").ToList();
					RequestsCollection.ItemsSource = filteredBoard;
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", "Unable to load board: " + ex.Message, "OK");
			}
		}

		private void OnLogoutClicked(object sender, EventArgs e)
		{
            // 1. We tell PocketBase to forget about us
            _pb.AuthStore.Clear();

            // 2. Delete the token from the phone vault
            SecureStorage.Default.Remove("auth_token");

            // 3. We redirect the user to the Login page
            Application.Current.MainPage = new NavigationPage(new LoginPage(_pb));
		}

		private async void OnNewRequestClicked(object sender, EventArgs e)
		{
			if (Navigation != null && Navigation.NavigationStack.Count > 0)
			{
				await Navigation.PushAsync(new NewRequestPage(_pb));
			}
			else
			{
				Application.Current.MainPage = new NavigationPage(new NewRequestPage(_pb));
			}
		}

		private async void OnRequestTapped(object sender, TappedEventArgs e)
		{
            // 1. We understand which "Card" (Frame) was touched by the user's finger
            var clickedFrame = (Frame)sender;

            // 2. We extract the data (the Request) that MAUI had linked to this specific Card
            var selectedRequest = clickedFrame.BindingContext as Request;

            // 3. If we have the data, let's go to the Details
            if (selectedRequest != null)
			{
				await clickedFrame.FadeTo(0.5, 100);
				await clickedFrame.FadeTo(1, 100);

				await Navigation.PushAsync(new RequestDetailsPage(selectedRequest, _pb));
			}


		}

		private async void OnProfileClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new ProfilePage(_pb));
		}

		private async void OnNotificationsClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new NotificationsPage(_pb));
		}
	}
}