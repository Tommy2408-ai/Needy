using pocketbase_csharp_sdk;
namespace Needy.Views;

public partial class HomePage : ContentPage
{
	private readonly PocketBase _pb;

	public HomePage(PocketBase pb)
	{
		InitializeComponent();
		_pb = pb;
	}

	private void OnLogoutClicked(object sender, EventArgs e)
	{
		// 1. Dichiariamo a PocketBase di scordarsi di noi
		_pb.AuthStore.Clear();

		// 2. Cancelliamo il token dalla cassaforte del telefono
		SecureStorage.Default.Remove("auth_token");

		// 3. Rimandiamo l'utente alla pagina di Login
		Application.Current.MainPage = new LoginPage(_pb);
	}
}