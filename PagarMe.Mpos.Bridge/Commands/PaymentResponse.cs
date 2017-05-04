﻿using PagarMe.Mpos.Devices;
using System;

namespace PagarMe.Mpos.Bridge.Commands
{
    public class PaymentResponse
    {
        public IDevice[] DeviceList { get; internal set; }

        public String CardHash { get; internal set; }

        public String Error { get; internal set; }

        public Type ResponseType { get; internal set; }

        public enum Type
        {
            UnknownCommand = 0,
            DevicesListed = 1,
            Initialized = 2,
            AlreadyInitialized = 3,
            Processed = 4,
            Finished = 5,
            MessageDisplayed = 6,
            Closed = 7,
            Error = 8,
        }
    }
}