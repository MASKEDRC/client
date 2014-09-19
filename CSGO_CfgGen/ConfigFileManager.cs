using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Linq;
using CSGO_CfgGen.Commands;

namespace CSGO_CfgGen
{
    public class ConfigFileManager
    {
        /// <summary>
        /// Enthält alle gefundenen Paths zu Configs welche geladen werden sollen.
        /// </summary>
        public List<string> cfgPaths = new List<string>();

        private List<ConfigFile> cfgFiles;

        public List<ConfigFile> CfgFiles
        {
            get { return cfgFiles; }
            set { cfgFiles = value; }
        }

        public ConfigFileManager()
        {
            this.cfgFiles = new List<ConfigFile>();
            findExistingConfigs();
        }

        /// <summary>
        /// Sucht über die Registry nach einer vorhandenen Autoexec.cfg
        /// und fügt diese der ConfigFile-Liste hinzu.
        /// </summary>
        private void findExistingConfigs()
        {
            const string csgoRegPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 730";
            string csgoPath = (string)Registry.GetValue(csgoRegPath, "InstallLocation", null);
            if (csgoPath != null)
            {
                string cfgPath = String.Format("{0}\\csgo\\cfg", csgoPath);
                string autoexecPath = String.Format("{0}\\autoexec.cfg", cfgPath);

                if (System.IO.File.Exists(autoexecPath))
                    this.cfgPaths.Add(autoexecPath);
            }
        }

        /// <summary>
        /// Startet das parsen einer ConfigFile rekursiv
        /// </summary>
        /// <param name="cfgId"></param>
        /// <param name="bParseFromFile">Gibt an, ob die existierende evtl geänderte File aus dem Speicher oder die physikalisch vorhandene File neu geparst werden soll</param>
        public void parseConfig(int cfgId, bool bParseFromFile)
        {
            ConfigFile cfgFile = this.cfgFiles.Where(cfg => cfg.Id.Equals(cfgId)).First();
            if (bParseFromFile)
                cfgFile.Commands = ConfigParser.parseConfigFile(cfgFile.Path);

            cfgFile.validateCommandos();

            //Beziehungen zu anderen Files aufbauen
            //Alle gültigen Exec-Commands holen
            IEnumerable<Commando> execCmds = cfgFile.Commands.Where(cmd => 
                cmd.CommandType == CommandType.Exec && 
                cmd.ValidationState == ValidationLevel.Ok);

            //bestehende Referenzen löschen
            cfgFile.SubConfigRef.Clear();

            foreach (Commando cmd in execCmds)
            {
                string path = cfgFile.Path.Substring(0, cfgFile.Path.LastIndexOf('\\') + 1);
                CExec execCmd = (CExec)cmd;
                path += execCmd.Filename;

                bool bLoopDetected = checkForCircle(path, cfgFile); 
                if (bLoopDetected)
                {
                    execCmd.LoopDetected = true;
                }
                else
                {
                    int id;
                    if (bParseFromFile)
                        id = addConfig(path, false);
                    else
                    {
                        ConfigFile file = getConfigByPath(path);
                        if (file != null)
                            id = file.Id;
                        else
                        {
                            id = addConfig(path, false);
                            bParseFromFile = true;
                        }
                    }
                    cfgFile.SubConfigRef.Add(new WeakReference(getConfigById(id)));
                    parseConfig(id, bParseFromFile);
                }
            }
        }

        /// <summary>
        /// Prüft den Weg Rückwärts auf Zyklen
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="cfgFile"></param>
        /// <returns></returns>
        public bool checkForCircle(string targetPath, ConfigFile cfgFile)
        {
            if (cfgFile.Path == targetPath)
                return true;

            IEnumerable<ConfigFile> prevFiles = CfgsWhichReferTo(cfgFile);
            return prevFiles.Any(cfg => checkForCircle(targetPath, cfg));
        }        

        private IEnumerable<ConfigFile> CfgsWhichReferTo(ConfigFile cfgFile)
        {
            return this.cfgFiles.Where(file => file.SubConfigRef.Any(cfgref => (cfgref.Target as ConfigFile).Id == cfgFile.Id));
        }

        /// <summary>
        /// Fügt dem Manager anhand eines Path eine ConfigFile hinzu.
        /// </summary>
        /// <param name="path">Gültiger Pfad zur Config Datei</param>
        /// <returns>ConfigFile-Id</returns>
        /// <param name="save">Soll der Path als standardPath hinzugefügt werden?</param>
        public int addConfig(string path, bool save)
        {
            if (save)
                cfgPaths.Add(path);

            ConfigFile newCfgFile = new ConfigFile(path, this.getCfgFileType(path));
            this.cfgFiles.Add(newCfgFile);
            return newCfgFile.Id;
        }

        private ConfigFileType getCfgFileType(string path)
        {
            if (path.EndsWith("autoexec.cfg"))
                return ConfigFileType.AUTOEXEC;
            else
                return ConfigFileType.OTHER;
        }

        public ConfigFile getConfigById(int id)
        {
            return this.cfgFiles.Find(cfg => cfg.Id.Equals(id));
        }

        public ConfigFile getConfigByPath(string path)
        {
            return this.cfgFiles.Find(cfg => cfg.Path.Equals(path));
        }

        ///// <summary>
        ///// Parst alle ConfigFile-Objekte neu.
        ///// </summary>
        //public void reparse()
        //{
        //    parseConfig(file.Id);            
        //}

        /// <summary>
        /// Löscht alle ConfigFiles und lädt übergebenen Path.
        /// </summary>
        public int load(string path)
        {
            this.CfgFiles.Clear();
            int id = this.addConfig(path, false);
            this.parseConfig(id, true);
            return id;
        }
    }
}
