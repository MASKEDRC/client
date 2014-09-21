using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSGO_CfgGen.Commands;

namespace CSGO_CfgGen.Commands
{
    public class CBind : Commando
    {
        private static readonly KeyToken[] keyboardToken = (KeyToken[])Enum.GetValues(typeof(KeyToken));

        /// <summary>
        /// The assigned key
        /// </summary>
        private KeyToken key;
        private string original_KeyStr;

        /// <summary>
        /// The assigned key
        /// </summary>
        public KeyToken Key
        {
            get { return key; }
            set { key = value; }
        }

        /// <summary>
        /// Commandos
        /// </summary>
        private Commando[] commands;

        /// <summary>
        /// Commandos
        /// </summary>
        public Commando[] Commands
        {
            get { return commands; }
            set { commands = value; }
        }

        public override string FullCommando
        {
            get 
            { 
                return String.Format("{0} {1} \"{2}\"",
                    this.commandType.ToString(),
                    this.original_KeyStr.ToString(),
                    String.Join(";", commands.Select(cmd => cmd.FullCommando))
                    ); 
            }
        }

        public CBind(string keyStr, Commando[] commands) : base(CommandType.bind)
        {
            this.original_KeyStr = keyStr;
            this.key = getKeyToken(keyStr);
            this.commands = commands;
        }

        /// <summary>
        /// Gibt aus einen String das entsprechende KeyToken zurück.
        /// </summary>
        /// <param name="keyStr"></param>
        /// <returns></returns>
        private KeyToken getKeyToken(string keyStr)
        {
            keyStr = keyStr.ToLower();
            return keyboardToken.FirstOrDefault(token => token.ToString().ToLower() == keyStr);
        }

        public override ValidationLevel validate()
        {
            this.validationState = ValidationLevel.Ok;

            //Key unbekannt
            if (this.key == KeyToken.UNKNOWN)
            {
                this.ValidationMessage = "Taste unbekannt!";
                this.validationState = ValidationLevel.Unknown;
            }

            return this.validationState;
        }
    }
}
