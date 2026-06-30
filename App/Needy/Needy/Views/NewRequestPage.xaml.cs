using Microsoft.Maui.Controls;
using Needy.Models;
using pocketbase_csharp_sdk;
using System;
using System.Transactions;

namespace Needy.Views
{
	public partial class NewRequestPage : ContentPage
	{
		private readonly PocketBase _pb;

		public NewRequestPage(PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
		}

		private async void OnPublishClicked(object sender, EventArgs e)
		{
            // 1. Let's make sure you don't send an empty request
            if (string.IsNullOrWhiteSpace(TitleEntry.Text) || string.IsNullOrWhiteSpace(DescriptionEditor.Text))
			{
				StatusLabel.Text = "Please fill in Title and Description!";
				StatusLabel.TextColor = Colors.Red;
				StatusLabel.IsVisible = true;
				return;
			}

			PublishButton.IsEnabled = false;
			PublishButton.Text = "SENDING...";
			StatusLabel.IsVisible = false;

			try
			{
                string myId = await SecureStorage.Default.GetAsync("my_id");

                // 1. Security check: if the session is empty, block everything
                if (string.IsNullOrEmpty(myId))
				{
					await DisplayAlert("Error", "You are not logged in. Please log in again.", "OK");
					return;
				}

                // 2. Prepare the duration
                double duration = 1;
				if (!string.IsNullOrWhiteSpace(DurationEntry.Text))
				{
					double.TryParse(DurationEntry.Text.Replace(".", ","), out duration);
				}

				var newRequest = new Request
				{
					title = TitleEntry.Text,
					description = DescriptionEditor.Text,
                    // ATTENTION: The category must be identical to the one on PocketBase.
                    category = CategoryPicker.SelectedItem?.ToString() ?? "Home",
					estimated_duration = duration,
					status = "OPEN",
					requester = myId,
					assistant = null,
					candidates = null
				};

                // 3. We send 
                var result = await _pb.Records.CreateAsync<Request>("requests", newRequest);

                // 4. Let's check the result
                if (result.IsSuccess) // If the result is not null, you have created the record!
                {
					await DisplayAlert("Fantastic!", "Your request for help has been published.", "OK");
					await Navigation.PopAsync();	
				}
				else
				{
					string error = result.Errors.Count > 0 ? result.Errors[0].Message : "Unknown error";
					await DisplayAlert("PocketBase refused!", error, "OK");

					StatusLabel.Text = "Error sending. Please try again.";
					StatusLabel.TextColor = Colors.Red;
					StatusLabel.IsVisible = true;
				}
			}
			catch (Exception ex)
			{
                await DisplayAlert("Error Detail", ex.Message, "OK");

				StatusLabel.Text = "Error sending. Please try again.";
				StatusLabel.TextColor = Colors.Red;
				StatusLabel.IsVisible = true;
			}
			finally
			{
				PublishButton.IsEnabled = true;
				PublishButton.Text = "PUBLISH REQUEST";
			}
		}
	}
}