using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSGO_CfgGen.Commands
{
    public class CUnknown : Commando
    {
        private string fullCommando;

        public override string FullCommando
        {
            get { return fullCommando; }
        }

        public CUnknown(string fullCommando) : base(CommandType.unknown)
        {
            this.fullCommando = fullCommando;
        }

        public override ValidationLevel validate()
        {
            this.ValidationMessage = "Befehl unbekannt!";
            return ValidationLevel.Unknown;
        }
        
    }
}
