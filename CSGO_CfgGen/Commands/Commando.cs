using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSGO_CfgGen.Commands
{
    public abstract class Commando
    {
        /// <summary>
        /// Gibt die Stelle Im Text an, an der der Befehl ist
        /// </summary>
        private int offset;

        /// <summary>
        /// Gibt die Stelle Im Text an, an der der Befehl ist
        /// </summary>
        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public string ValidationMessage = "";

        /// <summary>
        /// Gibt an, ob der Commando gültig ist
        /// </summary>
        protected ValidationLevel validationState;

        /// <summary>
        /// Gibt an, ob der Commando gültig ist
        /// </summary>
        public ValidationLevel ValidationState
        {
            get { return validationState; }
        }

        protected CommandType commandType;

        public CommandType CommandType
        {
            get { return commandType; }
        }

        public Commando(CommandType type)
        {
            this.commandType = type;
            this.validationState = ValidationLevel.Unknown;
        }

        /// <summary>
        /// Prüft, ob der Commando gültig ist.
        /// </summary>
        /// <returns>Gültigkeit</returns>
        public abstract ValidationLevel validate();

        /// <summary>
        /// Gibt das gesamte Commando zurück.
        /// </summary>
        public abstract string FullCommando { get; }
    }
}
