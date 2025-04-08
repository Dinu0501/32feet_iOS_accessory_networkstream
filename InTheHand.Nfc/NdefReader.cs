﻿// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Nfc.NdefReader
// 
// Copyright (c) 2020-2025 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Threading;
using System.Threading.Tasks;

namespace InTheHand.Nfc;

/// <preliminary/>
/// <summary>
/// Class to read NDEF formatted NFC tags.
/// </summary>
/// <remarks>See https://w3c.github.io/web-nfc/ for more details.</remarks>
public sealed partial class NdefReader : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Start scanning for NDEF tags.
    /// </summary>
    /// <param name="token">A CancellationToken to stop scanning.</param>
    /// <returns></returns>
    public Task ScanAsync(CancellationToken cancellationToken = default)
    {
        return PlatformScanAsync(cancellationToken);
    }

    /// <summary>
    /// Notify that a new reading is available.
    /// </summary>
    public event EventHandler<NdefReadingEventArgs> Reading;

    /// <summary>
    /// Notify that an error happened during reading.
    /// </summary>
    public event EventHandler Error;

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}