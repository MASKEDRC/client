using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSGO_CfgGen.Commands;

namespace CSGO_CfgGen.Commands
{
    public class CBind : Commando
    {
        private static string token = "bind";

        private string key;

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        private Commando[] commands;

        public Commando[] Commands
        {
            get { return commands; }
            set { commands = value; }
        }

        public override string FullCommando
        {
            get { return String.Format("{0} {1} \"{2}\"", token, key.ToString(), String.Join(";", commands.Select(cmd => cmd.FullCommando))); }
        }

        public CBind(string keyStr, Commando[] commands) : base(CommandType.Bind)
        {
            this.key = keyStr;
            this.commands = commands;
        }

        //private KeyToken getKeyToken(string keyStr)
        //{
        //    switch (keyStr)
        //    {
        //        case "KP_5":
        //            return KeyToken.KP_5;
        //        default:
        //            return KeyToken.Other;
        //    }
        //}

        public override ValidationLevel validate()
        {
            this.validationState = ValidationLevel.Unknown;

            //Key unbekannt
            if (this.key.Equals(KeyToken.Other))
            {
                this.ValidationMessage = "";
                this.validationState = ValidationLevel.Unknown;
            }

            return this.validationState;
        }
    }
}
