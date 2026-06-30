using FluentResults;
using Microsoft.Maui.Controls;
using Needy.Models;
using pocketbase_csharp_sdk;
using pocketbase_csharp_sdk.Helper.Convert;
using pocketbase_csharp_sdk.Models;
using pocketbase_csharp_sdk.Models.Auth;
using pocketbase_csharp_sdk.Services;
using System;

namespace Needy.Views
{
    public partial class LoginPage : ContentPage
    {
        // Variable for PocketBase that contains the connection
        private readonly PocketBase _pb;

        // When MAUI opens this page, it sees that it needs a "PocketBase".
        // It goes and gets it from MauiPrograms.cs and "injects" it into here via the (pb) variable.
        public LoginPage(PocketBase pb)
        {
            InitializeComponent();

            // We save the global connection in our private variable
            _pb = pb;
        }

        // This is the event that starts when you click "ENTER"
        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // We hide old mistakes and block the button
            ErrorLabel.IsVisible = false;
            LoginButton.IsEnabled = false;
            LoginButton.Text = "LOGIN IN PROGRESS...";

            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            try
            {
                // ACTION 2: THE CALL
                // We use <Needy.Models.User> to tell PocketBase to transform the data directly into the class
                var authData = await _pb.User.AuthenticateWithPasswordAsync(email, password);

                // ACTION 3: THE TRAFFIC LIGHT
                // Now that the data has arrived, we need to extract the user.

                var loggedInUser = authData.Value;

                if (loggedInUser != null && loggedInUser.Record.Verified == true)
                {
                    // GREEN! The Admin has accepted it.

                    // 1. Save the secret token in your phone's vault
                    await SecureStorage.Default.SetAsync("auth_token", _pb.AuthStore.Token);

                    await SecureStorage.Default.SetAsync("my_id", loggedInUser.Record.Id);

                    // We use MainThread to change pages and avoid crashes on Android.
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Application.Current.MainPage = new NavigationPage(new HomePage(_pb));
                    });
                }
                else
                {
                    // YELLOW! Password correct, but Admin hasn't accepted it yet (isVerified = false)
                    _pb.AuthStore.Clear(); // We disconnect it immediately for app security

                    ErrorLabel.Text = "Your account is pending approval by an administrator.";
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                // RED! Something went wrong (Wrong email, wrong password, or server down)
                // If you'd like to see the technical error for yourself, you can comment out the line below:
                // await DisplayAlert("System Error", ex.Message, "OK");

                ErrorLabel.Text = "Incorrect email or password, or server unreachable.";
                ErrorLabel.IsVisible = true;
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Text = "LOGIN";
            }
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistrationPage(_pb));
        }
    }
}