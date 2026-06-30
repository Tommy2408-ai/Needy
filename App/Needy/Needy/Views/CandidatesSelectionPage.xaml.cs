using Needy.Models;
using pocketbase_csharp_sdk;

namespace Needy.Views
{
	public partial class CandidatesSelectionPage : ContentPage
	{
		private readonly PocketBase _pb;
		private Request _currentRequest;
		public CandidatesSelectionPage(Request request, PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
			_currentRequest = request;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await LoadCandidates();
		}

		private async Task LoadCandidates()
		{
			try
			{
				var candidatesList = new List<CandidateInfo>();

				if (_currentRequest.candidates != null)
				{
					foreach (string candidateID in _currentRequest.candidates)
					{
						var response = await _pb.Records.GetOneAsync<User>("users", candidateID);
						if (response.IsSuccess && response.Value != null)
						{
							string writenMessage = "No message.";
							if (_currentRequest.candidate_messages != null && _currentRequest.candidate_messages.ContainsKey(candidateID))
								writenMessage = _currentRequest.candidate_messages[candidateID];

							candidatesList.Add(new CandidateInfo
							{
								Candidate = response.Value,
								Message = writenMessage
							});
						}
					}
				}

				CandidatesCollection.ItemsSource = candidatesList;
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", "Unable to load candidates.", "OK");
			}
		}

		private async void OnChooseCandidateClicked(object sender, EventArgs e)
		{
			var clickedButton = (Button)sender;

			var selectedInfo = clickedButton.BindingContext as CandidateInfo;
			if (selectedInfo == null) return;

			var selectedUser = selectedInfo.Candidate;

			bool isConfirmed = await DisplayAlert("Confirm", $"Do you want to assign the reques to {selectedUser.Name}?", "YES", "NO");

			if (isConfirmed)
			{
				clickedButton.IsEnabled = false;
				clickedButton.Text = "WAIT...";

				try
				{
					_currentRequest.assistant = selectedUser.Id;
					_currentRequest.status = "IN_PROGRESS";

					var result = await _pb.Records.UpdateAsync<Request>("requests", _currentRequest);

					if (result.IsSuccess)
					{
						await DisplayAlert("Done!", $"You have assigned the request to {selectedUser.Name}. The request is now IN PROGRESS.", "OK");

						await Navigation.PopToRootAsync();
					}
					else
					{
						string error = result.Errors.Count > 0 ? result.Errors[0].Message : "Unknown error";
						await DisplayAlert("PocketBase fault", error, "OK");
					}
				}
				catch (Exception ex)
				{
					await DisplayAlert("Error", ex.Message, "OK");
				}
				finally
				{
					clickedButton.IsEnabled = true;
					clickedButton.Text = "ACCEPT";
				}
			}
		}
	}
}