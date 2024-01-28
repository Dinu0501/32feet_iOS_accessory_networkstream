﻿namespace MauiWebBluetooth
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();

            
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var stat = await Permissions.RequestAsync<InTheHand.Bluetooth.Permissions.Bluetooth>();
            System.Diagnostics.Debug.WriteLine(stat);
            InTheHand.Bluetooth.Bluetooth.AvailabilityChanged += Bluetooth_AvailabilityChanged;
        }

        private async void Bluetooth_AvailabilityChanged(object sender, EventArgs e)
        {
            var a = await InTheHand.Bluetooth.Bluetooth.GetAvailabilityAsync();
            System.Diagnostics.Debug.WriteLine($"Availability: {a}");
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            var device = await InTheHand.Bluetooth.Bluetooth.RequestDeviceAsync();

            System.Diagnostics.Debug.WriteLine(device.Name);
        }
    }
}