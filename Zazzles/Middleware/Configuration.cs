﻿/*
 * Zazzles : A cross platform service framework
 * Copyright (C) 2014-2023 FOG Project
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

// ReSharper disable InconsistentNaming

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Zazzles.Middleware
{
    public static class Configuration
    {
        private const string LogName = "Middleware::Configuration";

        static Configuration()
        {
            ServerAddress = "";
            GetAndSetServerAddress();
        }

        public static string ServerAddress { get; set; }
        public static string TestMAC { get; set; }

        /// <summary>
        ///     Load the server information from the registry and apply it
        /// </summary>
        /// <returns>True if settings were updated</returns>
        public static bool GetAndSetServerAddress()
        {
            if (string.IsNullOrEmpty(Settings.Get("HTTPS")) || Settings.Get("WebRoot") == null ||
                string.IsNullOrEmpty(Settings.Get("Server")))
            {
                return false;
            }

            ServerAddress = (Settings.Get("HTTPS").Equals("1") ? "https://" : "http://");
            ServerAddress += Settings.Get("Server") +
                             Settings.Get("WebRoot");
            return true;
        }

        /// <summary>
        ///     Get the IP address of the host
        /// </summary>
        /// <returns>The first IP address of the host</returns>
        public static string IPAddress()
        {
            var hostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(hostName);
            var address = ipEntry.AddressList;

            return (address.Length > 0) ? address[0].ToString() : "";
        }

        /// <summary>
        ///     Get a string of all the host's valid MAC addresses
        /// </summary>
        /// <returns>A string of all the host's valid MAC addresses, split by |</returns>
        public static string MACAddresses()
        {
            if (!string.IsNullOrEmpty(TestMAC)) return TestMAC;

            var macs = "";
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();

                //filter to only ethernet and wifi adapter types that aren't bluetooth or virtual adapters
                //wanted to filter with the MSFT_NetworkAdapter ciminstance property of connectorPresent but that won't be universal to all OS and couldn't get it to come up
                //there are probably other strings to filter here
                //maybe an even better approach would be to have entries on the server for adapter descriptions to filter out of all client macs and have this pull from that list.
                var physicalAdapters =
                    from adapter in adapters
                    where ((adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet || adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && ((!adapter.Description.Contains("Microsoft Wi-Fi Direct Virtual Adapter"))) && ((!adapter.Description.Contains("VMware Virtual Ethernet Adapter for VMnet"))) && ((!adapter.Description.Contains("Bluetooth"))) && (adapter.Supports(NetworkInterfaceComponent.IPv4) == true))
                    select adapter;

                macs = physicalAdapters.Aggregate(macs, (current, adapter) =>
                    current +
                    ("|" +
                     string.Join(":",
                         (from z in adapter.GetPhysicalAddress().GetAddressBytes() select z.ToString("X2")).ToArray())));

                macs = macs.Trim('|');
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not get MAC addresses");
                Log.Error(LogName, ex);
            }

            return macs;
        }
    }
}
