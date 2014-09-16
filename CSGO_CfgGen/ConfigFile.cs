using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CSGO_CfgGen.Commands;

namespace CSGO_CfgGen
{
    public class ConfigFile
    {
        private static int count = 0;

        private readonly int id;

        public int Id
        {
            get { return id; }
        }

        private string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        private ConfigFileType type;

        public ConfigFileType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Die in der Config File entaltenen Commands
        /// </summary>
        private List<Commando> commands;

        /// <summary>
        /// Die in der Config File entaltenen Commands
        /// </summary>
        public List<Commando> Commands
        {
            get { return commands; }
            set { commands = value; }
        }

        /// <summary>
        /// Referenzen zu ConfigFiles, welche durch Exec-Commands aufgerufen werden.
        /// </summary>
        private List<WeakReference> subConfigRef;

        /// <summary>
        /// Referenzen zu ConfigFiles, welche durch Exec-Commands aufgerufen werden.
        /// </summary>
        public List<WeakReference> SubConfigRef
        {
            get { return subConfigRef; }
            set { subConfigRef = value; }
        }

        public ConfigFile(string cfgPath, ConfigFileType cfgType)
        {
            this.id = ++count;
            this.path = cfgPath;
            this.type = cfgType;
            this.subConfigRef = new List<WeakReference>();
        }

        public void parse()
        {
            this.commands = ConfigParser.parseConfigFile(this.path);
        }

        /// <summary>
        /// Gültigkeitsprüfung aller Commandos
        /// </summary>
        internal void validateCommandos()
        {
            foreach (Commando cmd in this.commands)
                cmd.validate();
        }

        /// <summary>
        /// Speichert die ConfigFile
        /// </summary>
        public void saveConfig()
        {
            //TODO
        }

        /// <summary>
        /// Prüft alle Commands und gibt den kritischsten ValidationLevel aus
        /// </summary>
        /// <returns></returns>
        internal ValidationLevel getValidationLevel()
        {
            ValidationLevel lvl = ValidationLevel.Ok;
            this.commands.ForEach(delegate(Commando cmd)
            {
                if(cmd.ValidationState > lvl)
                {
                    lvl = cmd.ValidationState;
                }
            });
            return lvl;
        }
    }
}
