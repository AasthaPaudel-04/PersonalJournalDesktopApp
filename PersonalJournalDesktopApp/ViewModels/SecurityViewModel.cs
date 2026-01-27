using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalJournalDesktopApp.Services;

namespace PersonalJournalDesktopApp.ViewModels
{
    public partial class SecurityViewModel : ObservableObject
    {
        private readonly SecurityService _securityService;

        [ObservableProperty]
        private bool isSecurityEnabled;

        [ObservableProperty]
        private string pinEntry = string.Empty;

        [ObservableProperty]
        private string confirmPinEntry = string.Empty;

        [ObservableProperty]
        private bool isSettingUp = false;

        public SecurityViewModel(SecurityService securityService)
        {
            _securityService = securityService;
        }

        public async Task InitializeAsync()
        {
            IsSecurityEnabled = await _securityService.IsSecurityEnabledAsync();
        }

        [RelayCommand]
        private async Task SetupPinAsync()
        {
            if (string.IsNullOrWhiteSpace(PinEntry))
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Please enter a PIN", "OK");
                return;
            }

            if (PinEntry.Length < 4)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "PIN must be at least 4 digits", "OK");
                return;
            }

            if (PinEntry != ConfirmPinEntry)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "PINs do not match", "OK");
                return;
            }

            await _securityService.EnableSecurityAsync(PinEntry);
            IsSecurityEnabled = true;

            PinEntry = string.Empty;
            ConfirmPinEntry = string.Empty;

            await Application.Current!.MainPage!.DisplayAlert("Success", "PIN protection enabled", "OK");
        }

        [RelayCommand]
        private async Task DisableSecurityAsync()
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Disable Security",
                "Are you sure you want to disable PIN protection?",
                "Yes",
                "No");

            if (confirm)
            {
                await _securityService.DisableSecurityAsync();
                IsSecurityEnabled = false;
                await Application.Current!.MainPage!.DisplayAlert("Success", "PIN protection disabled", "OK");
            }
        }

        [RelayCommand]
        private async Task ChangePinAsync()
        {
            if (string.IsNullOrWhiteSpace(PinEntry) || string.IsNullOrWhiteSpace(ConfirmPinEntry))
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Please enter a new PIN", "OK");
                return;
            }

            if (PinEntry.Length < 4)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "PIN must be at least 4 digits", "OK");
                return;
            }

            if (PinEntry != ConfirmPinEntry)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "PINs do not match", "OK");
                return;
            }

            await _securityService.ChangePinAsync(PinEntry);

            PinEntry = string.Empty;
            ConfirmPinEntry = string.Empty;

            await Application.Current!.MainPage!.DisplayAlert("Success", "PIN changed successfully", "OK");
        }
    }

    // Login ViewModel
    public partial class LoginViewModel : ObservableObject
    {
        private readonly SecurityService _securityService;

        [ObservableProperty]
        private string pinEntry = string.Empty;

        [ObservableProperty]
        private bool isAuthenticating = false;

        public LoginViewModel(SecurityService securityService)
        {
            _securityService = securityService;
        }

        [RelayCommand]
        private async Task AuthenticateAsync()
        {
            if (string.IsNullOrWhiteSpace(PinEntry))
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Please enter your PIN", "OK");
                return;
            }

            IsAuthenticating = true;

            try
            {
                var isValid = await _securityService.ValidatePinAsync(PinEntry);

                if (isValid)
                {
                    // Navigate to main app
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    await Application.Current.MainPage!.DisplayAlert("Error", "Incorrect PIN", "OK");
                    PinEntry = string.Empty;
                }
            }
            finally
            {
                IsAuthenticating = false;
            }
        }
    }
}