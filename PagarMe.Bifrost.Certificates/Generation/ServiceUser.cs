﻿using NLog;
using PagarMe.Generic;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PagarMe.Bifrost.Certificates.Generation
{
    public class ServiceUser
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        internal static Boolean GrantAccess(String path)
        {
            if (!Directory.Exists(path))
            {
                logger.Warn($"Could not grant access to {path}: directory do not exists");
                return false;
            }

            var info = new DirectoryInfo(path);
            var security = info.GetAccessControl();

            security.AddAccessRule(new FileSystemAccessRule(
                Get(),
                FileSystemRights.Read | FileSystemRights.Write,
                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                PropagationFlags.NoPropagateInherit,
                AccessControlType.Allow
            ));

            info.SetAccessControl(security);

            return true;
        }

        internal static void GrantLogAccess()
        {
            GrantAccess(logger.GetLogDirectoryPath());
        }

        internal static IdentityReference Get()
        {
            return new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        }
    }
}