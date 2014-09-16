using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSGO_CfgGen.Commands;
using System.Text.RegularExpressions;

namespace CSGO_CfgGen
{
    public class ConfigParser
    {
        /// <summary>
        /// Parst eine Config File vollständig mittels Stream Reader! 
        /// Und gibt die erkannten Commands zurück.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<Commando> parseConfigFile(string path)
        {
            List<Commando> commandlist = new List<Commando>();
            StreamReader reader = new StreamReader(path, Encoding.UTF8);
            string line; int offset = 0;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                commandlist.AddRange(parseLine(ref offset, line, path));
            }
            reader.Close();
            return commandlist;
        }

        public static List<Commando> parseLines(string[] lines, string path)
        {
            List<Commando> commandlist = new List<Commando>();
            int offset = 0;
            foreach (string line in lines)
            {
                commandlist.AddRange(parseLine(ref offset, line, path));
            }
            return commandlist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineOffset">Position aus einem Text, von der aus die Line beginnt</param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static List<Commando> parseLine(ref int lineOffset, string line, string path)
        {
            List<Commando> result = new List<Commando>();
            string[] splittedCMDs = line.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            List<string> finalCMDs = new List<string>();
            string cmdTemp = String.Empty;

            //Mehrere Cmds in einer Line erkennen
            foreach (string cmdStr in splittedCMDs)
            {
                cmdTemp += (String.IsNullOrEmpty(cmdTemp)?"":";") +  cmdStr;
                if (cmdTemp.Count(c => c == '\"') % 2 == 0) //Prüfung auf vollständigen Befehl
                {
                    finalCMDs.Add(cmdTemp);
                    cmdTemp = String.Empty;
                }
            }
            
            //Bei mehreren Cmds in einer Line
            //Die Leerzeichen umbauen
            if (finalCMDs.Count > 1)
            {
                for (int i = 1; i < finalCMDs.Count; i++)
                {
                    string cmd = finalCMDs[i];
                    string cmdNoSpaces = cmd.TrimStart(' ');
                    int anzSpace = cmd.Length - cmdNoSpaces.Length;
                    for (int s = 0; s < anzSpace; s++)
                        finalCMDs[i - 1] += " ";
                    finalCMDs[i] = cmdNoSpaces;
                }
            }

            //Command Objects erzeugen
            foreach (string cmdStr in finalCMDs)
            {
                string finalCMD = cmdStr;
                Commando cmd = createCommand(finalCMD, path);
                cmd.Offset = lineOffset;
                result.Add(cmd);
                lineOffset += finalCMD.Length;
            }
            return result;
        }

        private static Commando createCommand(string command, string path)
        {
            CommandType commType;
            string[] args = matchCommand(command, out commType);
            switch (commType)
            {
                case CommandType.Exec:
                    return new CExec(args[0], path.Substring(0, path.LastIndexOf('\\') + 1));
                case CommandType.Bind:
                    //parseLine(
                    //return new CBind(
                case CommandType.Log:
                case CommandType.UNKNOWN:
                default:
                    return new CUnknown(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd">zu parsendes Commando</param>
        /// <param name="cmdType"></param>
        /// <returns>Parameter-Array</returns>
        private static string[] matchCommand(string cmd, out CommandType cmdType)
        {
            Regex regex;
            string[] args = null;
            int anzParams = 0;
            cmdType = CommandType.UNKNOWN;

            foreach (KeyValuePair<CommandType, string> kvp in regexList)
            {
                regex = new Regex(kvp.Value);
                Match match = regex.Match(cmd);
                if (match.Success)
                {
                    cmdType = kvp.Key;
                    anzParams = match.Groups.Count - 1;
                    args = new string[anzParams];
                    for (int i = 1; i <= anzParams; i++)
                    {
                        args[i - 1] = match.Groups[i].Value;
                    }
                    break;
                }
            }
            return args;
        }

        #region RegEx-Liste
        private static Dictionary<CommandType, string> regexList = new Dictionary<CommandType, string>()
            {
                {CommandType.Bind, "^\\$"},//TODO
                {CommandType.Exec, @"^exec\s(.+)$"},
                {CommandType.Log, "^\\$"},//TODO
            };
        #endregion
    }
}
