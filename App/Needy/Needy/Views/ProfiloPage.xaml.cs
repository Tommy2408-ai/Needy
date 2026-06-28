using Needy.Models;
using pocketbase_csharp_sdk;

namespace Needy.Views
{
    public partial class ProfiloPage : ContentPage
    {
        private readonly PocketBase _pb;
        private List<Richiesta> _tutteLeRichiesteGrezze = new List<Richiesta>();
        private string _mioId = "";
        private bool _mostraLeMieRichieste = true; // true = Tab 1, false = Tab 2

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
                _mioId = await SecureStorage.Default.GetAsync("mio_id");

                if (string.IsNullOrEmpty(_mioId)) return;

                var mioProfilo = await _pb.Records.GetOneAsync<User>("users", _mioId);
                if (mioProfilo.IsSuccess && mioProfilo.Value != null)
                {
                    this.BindingContext = mioProfilo.Value;
                }

                var response = await _pb.Records.GetFullListAsync<Richiesta>("requests");
                if (response.IsSuccess && response.Value != null)
                {
                    _tutteLeRichiesteGrezze = response.Value.ToList();

                    ApplicaFiltri();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Unable to load profile data", "OK");
            }
        }

        private void OnTabClicked(object sender, EventArgs e)
        {
            var bottone = (Button)sender;

            if (bottone.Text == "My Requests")
            {
                _mostraLeMieRichieste = true;
                TabMyRequests.BackgroundColor = Color.FromArgb("#2196F3");
                TabMyRequests.TextColor = Colors.White;
                TabMyContributions.BackgroundColor = Colors.Transparent;
                TabMyContributions.TextColor = Colors.Gray;
            }
            else
            {
                _mostraLeMieRichieste = false;
                TabMyContributions.BackgroundColor = Color.FromArgb("#2196F3");
                TabMyContributions.TextColor = Colors.White;
                TabMyRequests.BackgroundColor = Colors.Transparent;
                TabMyRequests.TextColor = Colors.Gray;
            }

            ApplicaFiltri();
        }

        private void ApplicaFiltri()
        {
            var listaFiltrata = new List<Richiesta>();

            foreach (var r in _tutteLeRichiesteGrezze)
            {
                string req = r.requester ?? "";
                string ass = r.assistant ?? "";
                bool sonoCandidato = r.candidates != null && r.candidates.Contains(_mioId);

                if (_mostraLeMieRichieste)
                {
                    if (req == _mioId)
                    {
                        r.IsMyRequest = true;

                        if (r.status == "APERTA")
                        {
                            int numCandidati = r.candidates != null ? r.candidates.Count : 0;
                            if (numCandidati > 0)
                            {
                                r.StatoVisuale = $"🔔 {numCandidati} Candidate(s)!";
                                r.ColoreStato = "#FF9800";
                            }
                            else
                            {
                                r.StatoVisuale = "Waiting for help";
                                r.ColoreStato = "#9E9E9E";
                            }
                        }
                        else if (r.status == "IN_CORSO")
                        {
                            r.StatoVisuale = "🏃‍♂️ In Progress";
                            r.ColoreStato = "#4CAF50";
                        }
                        else if (r.status == "COMPLETATA")
                        {
                            r.StatoVisuale = "✅ Completed";
                            r.ColoreStato = "#2196F3";
                        }
                        listaFiltrata.Add(r);
                    }
                }
                else
                {
                    if (ass == _mioId || sonoCandidato)
                    {
                        r.IsMyRequest = false;

                        if (ass == _mioId)
                        {
                            if (r.status == "COMPLETATA")
                            {
                                r.StatoVisuale = "🌟 Help Provided!";
                                r.ColoreStato = "#2196F3";
                            }
                            else
                            {
                                r.StatoVisuale = "🤝 You are helping";
                                r.ColoreStato = "#4CAF50";
                            }
                        }
                        else if (sonoCandidato)
                        {
                            r.StatoVisuale = "⏳ Waiting for response";
                            r.ColoreStato = "#FF9800";
                        }

                        listaFiltrata.Add(r);
                    }
                }
            }

            StoricoCollection.ItemsSource = listaFiltrata;
        }

        private async void OnRichiestaTapped(object sender, EventArgs e)
        {
            var frameCliccato = (Frame)sender;
            var r = frameCliccato.BindingContext as Richiesta;

            if (r == null) return;

            await frameCliccato.FadeTo(0.5, 100);
            await frameCliccato.FadeTo(1, 100);

            if (r.IsMyRequest && r.status == "APERTA" && r.candidates != null && r.candidates.Count > 0)
            {
                await Navigation.PushAsync(new SceltaCandidatiPage(r, _pb));
            }
            else
            {
                await Navigation.PushAsync(new DettaglioRichiestaPage(r, _pb));
            }
        }
    }
}