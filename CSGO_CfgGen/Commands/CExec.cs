using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSGO_CfgGen.Commands
{
    public class CExec : Commando
    {
        private static string token = "exec";

        /// <summary>
        /// Path zum Directory in der die ConfigFile liegt, welche diesen Befehl enthält
        /// </summary>
        private string cfgDirPath;

        public override string FullCommando
        {
            get 
            {
                return String.Format("{0} {1}", token, filename);
            }
        }

        private string filename;

        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        /// <summary>
        /// Gibt an, ob ein Konflikt beim Aufruf von 
        /// ConfigDateien besteht. Endlosschleife.
        /// </summary>
        private bool loopDetected;

        /// <summary>
        /// Gibt an, ob ein Konflikt beim Aufruf von 
        /// ConfigDateien besteht. Endlosschleife.
        /// </summary>
        public bool LoopDetected
        {
            get { return loopDetected; }
            set { loopDetected = value; validate(); }
        }


        public CExec(string filename, string cfgDirPath) : base(CommandType.Exec)
        {
            this.filename = filename;
            this.cfgDirPath = cfgDirPath;
        }

        public override ValidationLevel validate()
        {
            ValidationLevel validationResult = ValidationLevel.Ok;
            ValidationMessage = "";

            //Warnungen:
            //File existiert nicht
            if (!System.IO.File.Exists(cfgDirPath + this.filename))
            {
                ValidationMessage = String.Format("Die Referenzierte Datei {0} existiert nicht!", this.filename);
                validationResult = ValidationLevel.Warning;
            }

            //Errors:
            //Loop wurde erkannt
            if(this.loopDetected)
            {
                ValidationMessage = String.Format("Die Referenz zur Datei {0} erzeugt eine Endlosschleife!", this.filename);
                validationResult = ValidationLevel.Error;
            }
            this.validationState = validationResult;
            return validationResult;
        }
    }
}
