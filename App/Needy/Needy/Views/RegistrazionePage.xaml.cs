using Needy.Models;
using pocketbase_csharp_sdk;

namespace Needy.Views
{
	public partial class RegistrazionePage : ContentPage
	{
		private readonly PocketBase _pb;

		private FileResult _documentoSelezionato;

		public RegistrazionePage(PocketBase pb)
		{
			InitializeComponent();
			_pb = pb;
		}

		private async void OnRegistratiClicked(object sender, EventArgs e)
		{
			ErrorLabel.IsVisible = false;

			if (_documentoSelezionato == null)
			{
				ErrorLabel.Text = "Devi caricare un documento d'identità per registrarti.";
				ErrorLabel.IsVisible = true;
				return;
			}

			if (string.IsNullOrWhiteSpace(NomeEntry.Text) || string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
			{
				ErrorLabel.Text = "Compila tutti i campo obbligatori.";
				ErrorLabel.IsVisible = true;
				return;
			}

			if (PasswordEntry.Text != ConfermaPasswordEntry.Text)
			{
				ErrorLabel.Text = "Le password non coincidono.";
				ErrorLabel.IsVisible = true;
				return;
			}

			if (PasswordEntry.Text.Length < 8)
			{
				ErrorLabel.Text = "La password deve essere di almeno 8 caratteri.";
				ErrorLabel.IsVisible = true;
				return;
			}

			RegistratiButton.IsEnabled = false;
			RegistratiButton.Text = "CREAZIONE IN CORSO...";

			try
			{
				var nuovoUtente = new User
				{
					Email = EmailEntry.Text,
					Password = PasswordEntry.Text,
					PasswordConfirm = ConfermaPasswordEntry.Text,
					Name = NomeEntry.Text,
					Neighborhood = QuartiereEntry.Text ?? "",
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
					await DisplayAlert("Bevenuto in Needy!", "Account creato con successo. Attendi che un amministratore verifichi il tuo profilo prima di poter fare il login.", "OK");

					await Navigation.PopAsync();
				}
				else
				{
					string errore = risultato.Errors.Count > 0 ? risultato.Errors[0].Message : "Errore sconosciuto";
					ErrorLabel.Text = errore;
					ErrorLabel.IsVisible = true;
				}
			}
			catch (Exception ex)
			{
				ErrorLabel.Text = "Impossibile creare l'account. Forse l'email è già in uso?";
				ErrorLabel.IsVisible = true;
			}
			finally
			{
				RegistratiButton.IsEnabled = true;
				RegistratiButton.Text = "REGISTRATI";
			}
		}

		private async void OnScegliDocumentoClicked(object sender, EventArgs e)
		{
			try
			{
				var risultato = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
				{
					Title = "Seleziona la foto del documento"
				});

				if (risultato != null)
				{
					_documentoSelezionato = risultato;

					NomeDocumentoLabel.Text = $"File: {risultato.FileName}";
					NomeDocumentoLabel.TextColor = Colors.Green;
					ScegliDocumentoButton.BackgroundColor = Color.FromArgb("#A5D6A7");
                }
			}
			catch (Exception ex)
			{

			}
		}

		private async void OnTornaAlLoginTapped(object sender, EventArgs e)
		{
			await Navigation.PopAsync();
		}
	}
}