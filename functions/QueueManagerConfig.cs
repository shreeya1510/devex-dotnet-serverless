using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace devex
{
    public class QueueManagerConfig
    {
        public static Dictionary<string, string> GetQmgrSettings(string qmgrName)
        {
            
            for (int i = 0; i < 10; i++)
            {
                string qmgrKey = GetEnvironmentVariable($"QMGR__{i}");
                Console.WriteLine("Qmgr = {0}", qmgrKey);
                 Console.WriteLine("QmgrName = {0}", qmgrName);

                if (qmgrKey == null)
                {
                    Console.WriteLine($"Environment variable QMGR__{i} is not set.");
                    continue;  // Skip this iteration if the qmgrKey is null
                }

                if (qmgrKey == qmgrName)
                {
                    // Fetch the settings and check for null values
                    var host = GetEnvironmentVariable($"HOST__{i}");
                    var port = GetEnvironmentVariable($"PORT__{i}");
                    var channel = GetEnvironmentVariable($"CHANNEL__{i}");
                    var user = GetEnvironmentVariable($"APP_USER__{i}");
                    var password = GetEnvironmentVariable($"APP_PASSWORD__{i}");
                    var cipher = GetEnvironmentVariable($"CIPHER_SPEC__{i}");
                    var keyRepository = GetEnvironmentVariable($"KEY_REPOSITORY__{i}");

                    // Log if any of the values are null
                    if (host == null || port == null || channel == null || user == null || password == null || cipher == null || keyRepository == null)
                    {
                        Console.WriteLine("One or more environment variables are missing.");
                    }

                    return new Dictionary<string, string>
                    {
                        ["HOST"] = host ?? "defaultHost",  // Provide a fallback or default value
                        ["PORT"] = port ?? "defaultPort",  // Provide a fallback or default value
                        ["CHANNEL"] = channel ?? "defaultChannel",  // Provide a fallback or default value
                        ["APP_USER"] = user ?? "defaultUser",  // Provide a fallback or default value
                        ["APP_PASSWORD"] = password ?? "defaultPassword",  // Provide a fallback or default value
                        ["CIPHER_SPEC"] = cipher ?? "defaultCipher",  // Provide a fallback or default value
                        ["KEY_REPOSITORY"] = keyRepository ?? "defaultKeyRepository"  // Provide a fallback or default value
                    };
                }
            }

            // Log when no matching queue manager is found
            Console.WriteLine($"No queue manager found with the name: {qmgrName}");
            return null;
        }
         private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
