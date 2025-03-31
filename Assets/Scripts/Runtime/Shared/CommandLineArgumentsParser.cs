using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.DedicatedServer;

namespace RedGaint.Network.Runtime
{
    internal class CommandLineArgumentsParser
    {
        public int Port { get; }
        
        const int k_DefaultPort = 7777;
        public int TargetFramerate { get; }
        const int k_DefaultTargetFramerate = 30;

        readonly string[] m_Args;
        Dictionary<string, Action<string>> m_CommandDictionary = new Dictionary<string, Action<string>>();

        /// <summary>
        /// Initializes the CommandLineArgumentsParser
        /// </summary>
        public CommandLineArgumentsParser() : this(Environment.GetCommandLineArgs()) { }
        
        /// <summary>
        /// Initializes the CommandLineArgumentsParser
        /// </summary>
        /// <param name="arguments">Arguments to process</param>
        // public CommandLineArgumentsParser(string[] arguments)
        // {
        //     m_Args = arguments;
        //     if (m_Args == null) // Android fix
        //     {
        //         m_Args = new string[0];
        //     }
        //
        //     Port = Arguments.Port.HasValue ? Arguments.Port.Value : k_DefaultPort;
        //     TargetFramerate = Arguments.TargetFramerate.HasValue ? Arguments.TargetFramerate.Value : k_DefaultTargetFramerate;
        // }
        //
        const string k_IPCmd = "ip";
        const string k_PortCmd = "port";
        const string k_QueryPortCmd = "queryPort";
        public static string IP()
        {
            return PlayerPrefs.GetString(k_IPCmd);
        }

        public static int _Port()
        {
            return PlayerPrefs.GetInt(k_PortCmd);
        }

        public static int QPort()
        {
            return PlayerPrefs.GetInt(k_QueryPortCmd);
        }
        void SetIP(string ipArgument)
        {
            PlayerPrefs.SetString(k_IPCmd, ipArgument);
        }
        void SetPort(string portArgument)
        {
            if (int.TryParse(portArgument, out int parsedPort))
            {
                PlayerPrefs.SetInt(k_PortCmd, parsedPort);
            }
            else
            {
                Debug.LogError($"{portArgument} does not contain a parseable port!");
            }
        }
        void SetQueryPort(string qPortArgument)
        {
            if (int.TryParse(qPortArgument, out int parsedQPort))
            {
                PlayerPrefs.SetInt(k_QueryPortCmd, parsedQPort);
            }
            else
            {
                Debug.LogError($"{qPortArgument} does not contain a parseable query port!");
            }
        }

        public void Initialise()
        {
            SetIP("127.0.0.1");
            SetPort("7777");
            SetQueryPort("7787");
            m_CommandDictionary["-" + k_IPCmd] = SetIP;
            m_CommandDictionary["-" + k_PortCmd] = SetPort;
            m_CommandDictionary["-" + k_QueryPortCmd] = SetQueryPort;
        }
        
        public CommandLineArgumentsParser(string[] args)
        {
            Initialise();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Launch Args: ");
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var nextArg = "";
                if (i + 1 < args.Length) // if we are evaluating the last item in the array, it must be a flag
                    nextArg = args[i + 1];

                if (EvaluatedArgs(arg, nextArg))
                {
                    sb.Append(arg);
                    sb.Append(" : ");
                    sb.AppendLine(nextArg);
                    i++;
                }
            }
            // Port = Arguments.Port.HasValue ? Arguments.Port.Value : k_DefaultPort;
            Port = _Port();
            TargetFramerate = Arguments.TargetFramerate.HasValue ? Arguments.TargetFramerate.Value : k_DefaultTargetFramerate;
            Debug.Log(sb);
        }
        bool EvaluatedArgs(string arg, string nextArg)
        {
            if (!IsCommand(arg))
                return false;
            if (IsCommand(nextArg)) // If you have need for flags, make a separate dict for those.
            {
                return false;
            }

            m_CommandDictionary[arg].Invoke(nextArg);
            return true;
        }
        bool IsCommand(string arg)
        {
            return !string.IsNullOrEmpty(arg) && m_CommandDictionary.ContainsKey(arg) && arg.StartsWith("-");
        }
        // /// <summary>
        // /// Extracts a value for command line arguments provided
        // /// </summary>
        // /// <param name="argName"></param>
        // /// <param name="defaultValue"></param>
        // /// <param name="argumentAndValueAreSeparated"></param>
        // /// <returns></returns>
        // string ExtractValue(string argName, string defaultValue = null, bool argumentAndValueAreSeparated = true)
        // {
        //     if (argumentAndValueAreSeparated)
        //     {
        //         if (!m_Args.Contains(argName))
        //         {
        //             return defaultValue;
        //         }
        //
        //         var index = m_Args.ToList().FindIndex(0, a => a.Equals(argName));
        //         return m_Args[index + 1];
        //     }
        //
        //     foreach (var argument in m_Args)
        //     {
        //         if (argument.StartsWith(argName)) //I.E: "-epiclocale=it"
        //         {
        //             return argument.Substring(argName.Length + 1, argument.Length - argName.Length - 1);
        //         }
        //     }
        //     return defaultValue;
        // }
        //
        // int ExtractValueInt(string argName, int defaultValue = -1)
        // {
        //     var number = ExtractValue(argName, defaultValue.ToString());
        //     return Convert.ToInt32(number);
        // }
    }
}
