using Microsoft.Maui.Controls;
using Needy.Models;
using pocketbase_csharp_sdk;
using System;
using System.Diagnostics;

namespace Needy.Views
{
	public partial class RequestDetailsPage : ContentPage
	{
		private readonly PocketBase _pb;
		private Request _currentRequest;

		public RequestDetailsPage(Request request, PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
			_currentRequest = request;

			this.BindingContext = _currentRequest;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			LoadingOverlay.IsVisible = true;

			try
			{
				var author = await _pb.Records.GetOneAsync<User>("users", _currentRequest.requester);

				if (author.IsSuccess && author.Value != null)
				{
					AuthorCard.BindingContext = author.Value;
					AuthorCard.IsVisible = true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error loading author: {ex.Message}");
			}

			string myId = await SecureStorage.Default.GetAsync("my_id");

			if (_currentRequest.status == "COMPLETED")
				CompletedLabel.IsVisible = true;
			else if (_currentRequest.requester == myId)
			{
				if (_currentRequest.status == "IN_PROGRESS")
					CompleteButton.IsVisible = true;
			}
			else
			{
				if (_currentRequest.status == "OPEN")
				{
					AcceptButton.IsVisible = true;

					if (_currentRequest.candidates != null && _currentRequest.candidates.Contains(myId))
					{
						AcceptButton.Text = "ALREADY CANDIDATE";
						AcceptButton.BackgroundColor = new Color(216, 138, 74);
						AcceptButton.IsEnabled = false;

                    }
				}
			}

            LoadingOverlay.IsVisible = false;
        }

		private async void OnAuthorTapped(object sender, EventArgs e)
		{
			var card = (Frame)sender;
			var clickedUser = card.BindingContext as User;

			if (clickedUser != null)
			{
				await card.FadeTo(0.5, 100);
				await card.FadeTo(1, 100);

				await DisplayAlert("Profile", $"Do you want to see the profile of {clickedUser.Name}?\nHe donated {clickedUser.reputation_hour} hrs!", "OK");
			}
		}

		private async void OnAcceptClicked(object sender, EventArgs e)
		{
			string myId = await SecureStorage.Default.GetAsync("my_id");

			if (string.IsNullOrEmpty(myId))
			{
				await DisplayAlert("Error", "Session expired. You must log in to accept.", "OK");
				return;
			}

            // Logical Check: You cannot accept YOUR OWN request
            if (_currentRequest.requester == myId)
			{
				await DisplayAlert("Hey!", "You cannot volunteer for a request you created yourself..", "OK");
				return;
			}

            // Verify that the object has a valid ID
            if (string.IsNullOrEmpty(_currentRequest.Id))
			{
				await DisplayAlert("Error", "Invalid request ID. Reload the page.", "OK");
				return;
			}

			AcceptButton.IsEnabled = false; // Disable the button to avoid multiple clicks
            AcceptButton.Text = "ACCEPTANCE IN PROGRESS...";

			try
			{
                // Let's check if you have already applied
                if (_currentRequest.candidates != null && _currentRequest.candidates.Contains(myId))
				{
					await DisplayAlert("Hey!", "You've already applied for this request. Please wait for a response from the person who created it.", "OK");
					return;
				}

				string message = await DisplayPromptAsync(
					title: "You are offering!",
					message: "Write a short message to the author (optional):",
					accept: "SEND",
					cancel: "CANCEL",
					placeholder: "Ex: Hi, I can come around 3:00 pm");

				if (message == null)
				{
					AcceptButton.IsEnabled = true;
					AcceptButton.Text = "I CAN HELP";
					return;
				}

				AcceptButton.IsEnabled = false;
				AcceptButton.Text = "APPLICATION IN PROGRESS...";

				if (_currentRequest.candidates == null)
				{
					_currentRequest.candidates = new List<string>();
				}

				_currentRequest.candidates.Add(myId);

				if (_currentRequest.candidate_messages == null)
					_currentRequest.candidate_messages = new Dictionary<string, string>();

				_currentRequest.candidate_messages[myId] = message;

                // Note: We are NOT changing the status. The request remains "Open" so others can apply.
                // Note: We do NOT fill in "assistant." The requester will fill that in later.

                var result = await _pb.Records.UpdateAsync<Request>("requests", _currentRequest);

				if (result.IsSuccess)
				{
					await DisplayAlert("Application sent!! 🙋‍♂️", "You've applied for this request. The author will be notified and can choose you.", "OK");
                    AcceptButton.Text = "ALREADY CANDIDATE";
                    AcceptButton.BackgroundColor = new Color(216, 138, 74);
                }
				else
				{
					string error = result.Errors.Count > 0 ? result.Errors[0].Message : "Unknown error";
					await DisplayAlert("Impossible to apply", error, "OK");

					_currentRequest.candidates.Remove(myId);
					_currentRequest.candidate_messages.Remove(myId);
				}
			}
			catch (Exception ex)
			{
                // Full exception log for debugging
                string errorMessage = $"Error: {ex.GetType().Name}\n\n{ex.Message}";
				if (ex.InnerException != null)
				{
					errorMessage += $"\n\nInner Exception: {ex.InnerException.Message}";
				}

				await DisplayAlert("Network Error", errorMessage, "OK");

				_currentRequest.status = "OPEN";
				_currentRequest.assistant = null;
			}
			finally
			{
				
			}
		}

		private async void OnCompleteClicked(object sender, EventArgs e)
		{
			bool confirmation = await DisplayAlert("Confirm", "Do you want to close this request? The hours will be assigned to the assistant.", "YES", "NO");
			if (!confirmation) return;

			CompleteButton.IsEnabled = false;
			CompleteButton.Text = "LOADING";

			try
			{
				_currentRequest.status = "COMPLETED";
				await _pb.Records.UpdateAsync<Request>("requests", _currentRequest);

				if (!string.IsNullOrEmpty(_currentRequest.assistant))
				{
					var helper = await _pb.Records.GetOneAsync<User>("users", _currentRequest.assistant);

					if (helper.IsSuccess && helper.Value != null)
					{
						helper.Value.Id = _currentRequest.assistant;

						helper.Value.reputation_hour += (int)Math.Ceiling(_currentRequest.estimated_duration);

						var updateUser = await _pb.Records.UpdateAsync<User>("users", helper.Value);

						if (!updateUser.IsSuccess)
						{
							string err = updateUser.Errors.Count > 0 ? updateUser.Errors[0].Message : "Unknown error";
							await DisplayAlert("Error Hrs", $"Unable to update hrs: {err}", "OK");
						}
					}
				}

				await DisplayAlert("Done!", "Request completed and hours assigned! Thank you for using Needy.", "OK");
				await Navigation.PopAsync();
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", "Unable to complete your request.", "OK");
				_currentRequest.status = "IN_PROGRESS";
				CompleteButton.IsEnabled = true;
				CompleteButton.Text = "✓ MARK AS COMPLETED";

            }
		}
    }
}