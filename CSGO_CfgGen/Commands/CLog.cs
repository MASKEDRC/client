using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSGO_CfgGen.Commands
{
    public class CLog : Commando
    {
        public CLog() : base(CommandType.log)
        {
        }

        public override ValidationLevel validate()
        {
            this.validationState = ValidationLevel.Unknown;
            return ValidationLevel.Unknown; //TODO
        }

        public override string FullCommando
        {
            get { throw new NotImplementedException(); } //TODO
        }
    }
}
