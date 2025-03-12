﻿//-----------------------------------------------------------------------
// <copyright file="BluetoothRemoteGATTServer.windows.cs" company="In The Hand Ltd">
//   Copyright (c) 2018-25 In The Hand Ltd, All rights reserved.
//   This source code is licensed under the MIT License - see License.txt
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

namespace InTheHand.Bluetooth
{
    partial class RemoteGattServer
    {
        private void PlatformInit()
        {
            Device.NativeDevice.ConnectionStatusChanged += NativeDevice_ConnectionStatusChanged;
        }

        private void NativeDevice_ConnectionStatusChanged(Windows.Devices.Bluetooth.BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == Windows.Devices.Bluetooth.BluetoothConnectionStatus.Disconnected)
                Device.OnGattServerDisconnected();
        }

        private bool GetConnected()
        {
            if (Device.IsDisposedItem(Device)) return false;
            return Device.NativeDevice.ConnectionStatus == Windows.Devices.Bluetooth.BluetoothConnectionStatus.Connected;
        }

        private async Task PlatformConnect()
        {
            // Ensure that our native objects have not been disposed.
            // If they have, re-create the native device object.
            if (await Device.CreateNativeInstance()) PlatformInit();

            var status = await Device.NativeDevice.RequestAccessAsync();
            if (status == Windows.Devices.Enumeration.DeviceAccessStatus.Allowed)
            {
                Device.LastKnownAddress = Device.NativeDevice.BluetoothAddress;
                var session = await Windows.Devices.Bluetooth.GenericAttributeProfile.GattSession.FromDeviceIdAsync(Device.NativeDevice.BluetoothDeviceId);
                if (session != null)
                {
                    Mtu = session.MaxPduSize;
                    session.MaxPduSizeChanged += Session_MaxPduSizeChanged;
                    // Even though this is a local variable, we still want to add it to our dispose list so
                    // we don't have to rely on the GC to clean it up.
                    Device.AddDisposableObject(this, session);

                    if (session.CanMaintainConnection)
                        session.MaintainConnection = true;
                }

                // need to request something to force a connection
                for (int i = 0; i < 3; i++)
                {
                    var services = await Device.NativeDevice.GetGattServicesForUuidAsync(GattServiceUuids.GenericAccess, Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);
                    if (services.Status == Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus.Success)
                    {
                        foreach (var service in services.Services)
                        {
                            service.Dispose();
                        }
                        break;
                    }
                }
            }
            else
            {
                throw new SecurityException();
            }
        }

        private void Session_MaxPduSizeChanged(Windows.Devices.Bluetooth.GenericAttributeProfile.GattSession sender, object args)
        {
            System.Diagnostics.Debug.WriteLine($"MaxPduSizeChanged Size:{sender.MaxPduSize}");
            Mtu = sender.MaxPduSize;
        }

        private void PlatformDisconnect()
        {
            // Windows has no explicit disconnect 🤪
        }

        private void PlatformCleanup()
        {
            // The user has explicitly called the Disconnect method so unhook ConnectionStatusChanged
            // and dispose all of the native windows bluetooth objects.  This will release the device
            // so that it can be used by another application or re-connected by the current
            // application.
            if (Device.NativeDisposeList.TryGetValue(Device.GetHashCode(), out IDisposable existingDevice))
            {
                if (existingDevice != null)
                {
                    Device.NativeDevice.ConnectionStatusChanged -= NativeDevice_ConnectionStatusChanged;
                    Device.DisposeAllNativeObjects();
                }
            }
        }

        private async Task<GattService> PlatformGetPrimaryService(BluetoothUuid service)
        {
            if (await Device.CreateNativeInstance()) PlatformInit();
            var result = await Device.NativeDevice.GetGattServicesForUuidAsync(service, Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);

            if (result == null || result.Services.Count == 0)
                return null;

            return new GattService(Device, result.Services[0], true);
        }

        private async Task<List<GattService>> PlatformGetPrimaryServices(BluetoothUuid? service)
        {
            if (await Device.CreateNativeInstance()) PlatformInit();
            var services = new List<GattService>();
            var nativeDevice = Device.NativeDevice;
            Windows.Devices.Bluetooth.BluetoothCacheMode cacheMode = nativeDevice.DeviceInformation.Pairing.IsPaired ? Windows.Devices.Bluetooth.BluetoothCacheMode.Cached : Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached;
            Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceServicesResult result;
            if (service == null)
            {
                result = await nativeDevice.GetGattServicesAsync(cacheMode);
            }
            else
            {
                result = await nativeDevice.GetGattServicesForUuidAsync(service.Value, cacheMode);
            }

            if (result != null && result.Services.Count > 0)
            {
                foreach(var serv in result.Services)
                {
                    services.Add(new GattService(Device, serv, true));
                }
            }
            
            return services;
        }

        private Task<short> PlatformReadRssi()
        {
            return Task.FromResult((short)0);
        }

        private void PlatformSetPreferredPhy(BluetoothPhy phy)
        {
        }

        private Task<bool> PlatformRequestMtuAsync(int mtu)
        {
            return Task.FromResult(false);
        }
    }
}