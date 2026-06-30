using Needy.Models;
using pocketbase_csharp_sdk;

namespace Needy.Views
{
    public partial class RegistrationPage : ContentPage
    {
        private readonly PocketBase _pb;

        private FileResult _documentoSelezionato;

        public RegistrationPage(PocketBase pb)
        {
            InitializeComponent();
            _pb = pb;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;

            if (_documentoSelezionato == null)
            {
                ErrorLabel.Text = "You must upload an ID to register.";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(NameEntry.Text) || string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                ErrorLabel.Text = "Fill in all required fields.";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                ErrorLabel.Text = "The passwords do not match.";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (PasswordEntry.Text.Length < 8)
            {
                ErrorLabel.Text = "Password must be at least 8 characters long.";
                ErrorLabel.IsVisible = true;
                return;
            }

            RegisterButton.IsEnabled = false;
            RegisterButton.Text = "CREATING IN PROGRESS...";

            try
            {
                var nuovoUtente = new User
                {
                    Email = EmailEntry.Text,
                    Password = PasswordEntry.Text,
                    PasswordConfirm = ConfirmPasswordEntry.Text,
                    Name = NameEntry.Text,
                    Neighborhood = NeighborhoodEntry.Text ?? "",
                    reputation_hour = 0
                };

                using var stream = await _documentoSelezionato.OpenReadAsync();

                var fileDaCaricare = new pocketbase_csharp_sdk.Models.Files.StreamFile()
                {
                    FileName = _documentoSelezionato.FileName,
                    Stream = stream,
                    FieldName = "id_document"
                };

                var files = new List<pocketbase_csharp_sdk.Models.Files.IFile>
                {
                    fileDaCaricare
                };

                var risultato = await _pb.Records.CreateAsync<User>("users", nuovoUtente, files: files);

                if (risultato.IsSuccess)
                {
                    await DisplayAlert("Welcome to Needy!", "Account created successfully. Please wait for an administrator to verify your profile before you can log in.", "OK");

                    await Navigation.PopAsync();
                }
                else
                {
                    string errore = risultato.Errors.Count > 0 ? risultato.Errors[0].Message : "Unknown error";
                    ErrorLabel.Text = errore;
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = "Unable to create account. Perhaps the email address is already in use?";
                ErrorLabel.IsVisible = true;
            }
            finally
            {
                RegisterButton.IsEnabled = true;
                RegisterButton.Text = "SIGN UP";
            }
        }

        private async void OnChooseDocumentClicked(object sender, EventArgs e)
        {
            try
            {
                var risultato = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select the document photo"
                });

                if (risultato != null)
                {
                    _documentoSelezionato = risultato;

                    DocumentNameLabel.Text = $"File: {risultato.FileName}";
                    DocumentNameLabel.TextColor = Colors.Green;
                    ChooseDocumentButton.BackgroundColor = Color.FromArgb("#A5D6A7");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void OnBackToLoginTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}