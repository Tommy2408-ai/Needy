using Needy.Models;
using pocketbase_csharp_sdk;

namespace Needy.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly PocketBase _pb;
        private List<Request> _allRawRequests = new List<Request>();
        private string _myId = "";
        private bool _showMyRequests = true; // true = Tab 1, false = Tab 2

        public ProfilePage(PocketBase pb)
        {
            InitializeComponent();
            _pb = pb;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            LoadingOverlay.IsVisible = true;
            await LoadProfileData();
            LoadingOverlay.IsVisible = false;
        }

        private async Task LoadProfileData()
        {
            try
            {
                _myId = await SecureStorage.Default.GetAsync("my_id");

                if (string.IsNullOrEmpty(_myId)) return;

                var myProfile = await _pb.Records.GetOneAsync<User>("users", _myId);
                if (myProfile.IsSuccess && myProfile.Value != null)
                {
                    this.BindingContext = myProfile.Value;
                }

                var response = await _pb.Records.GetFullListAsync<Request>("requests");
                if (response.IsSuccess && response.Value != null)
                {
                    _allRawRequests = response.Value.ToList();

                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Unable to load profile data", "OK");
            }
        }

        private void OnTabClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.Text == "My Requests")
            {
                _showMyRequests = true;
                MyRequestsTab.BackgroundColor = Color.FromArgb("#2196F3");
                MyRequestsTab.TextColor = Colors.White;
                MyContributionsTab.BackgroundColor = Colors.Transparent;
                MyContributionsTab.TextColor = Colors.Gray;
            }
            else
            {
                _showMyRequests = false;
                MyContributionsTab.BackgroundColor = Color.FromArgb("#2196F3");
                MyContributionsTab.TextColor = Colors.White;
                MyRequestsTab.BackgroundColor = Colors.Transparent;
                MyRequestsTab.TextColor = Colors.Gray;
            }

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filteredList = new List<Request>();

            foreach (var r in _allRawRequests)
            {
                string req = r.requester ?? "";
                string assist = r.assistant ?? "";
                bool isCandidate = r.candidates != null && r.candidates.Contains(_myId);

                if (_showMyRequests)
                {
                    if (req == _myId)
                    {
                        r.IsMyRequest = true;

                        if (r.status == "OPEN")
                        {
                            int candidateCount = r.candidates != null ? r.candidates.Count : 0;
                            if (candidateCount > 0)
                            {
                                r.VisualState = $"🔔 {candidateCount} Candidate(s)!";
                                r.StateColor = "#FF9800";
                            }
                            else
                            {
                                r.VisualState = "Waiting for help";
                                r.StateColor = "#9E9E9E";
                            }
                        }
                        else if (r.status == "IN_PROGRESS")
                        {
                            r.VisualState = "🏃‍♂️ In Progress";
                            r.StateColor = "#4CAF50";
                        }
                        else if (r.status == "COMPLETED")
                        {
                            r.VisualState = "✅ Completed";
                            r.StateColor = "#2196F3";
                        }
                        filteredList.Add(r);
                    }
                }
                else
                {
                    if (assist == _myId || isCandidate)
                    {
                        r.IsMyRequest = false;

                        if (assist == _myId)
                        {
                            if (r.status == "COMPLETED")
                            {
                                r.VisualState = "🌟 Help Provided!";
                                r.StateColor = "#2196F3";
                            }
                            else
                            {
                                r.VisualState = "🤝 You are helping";
                                r.StateColor = "#4CAF50";
                            }
                        }
                        else if (isCandidate)
                        {
                            r.VisualState = "⏳ Waiting for response";
                            r.StateColor = "#FF9800";
                        }

                        filteredList.Add(r);
                    }
                }
            }

            HistoryCollection.ItemsSource = filteredList;
        }

        private async void OnRequestTapped(object sender, EventArgs e)
        {
            var clickedFrame = (Frame)sender;
            var r = clickedFrame.BindingContext as Request;

            if (r == null) return;

            await clickedFrame.FadeTo(0.5, 100);
            await clickedFrame.FadeTo(1, 100);

            if (r.IsMyRequest && r.status == "OPEN" && r.candidates != null && r.candidates.Count > 0)
            {
                await Navigation.PushAsync(new CandidatesSelectionPage(r, _pb));
            }
            else
            {
                await Navigation.PushAsync(new RequestDetailsPage(r, _pb));
            }
        }
    }
}