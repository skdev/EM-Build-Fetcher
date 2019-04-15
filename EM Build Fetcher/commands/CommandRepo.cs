using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Build_Fetcher.commands
{
    public class CommandRepo
    {
        private readonly Dictionary<string, Command> _commands = new Dictionary<string, Command>();

        public bool Add(string name, Command command)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException();
            }

            if (command == null)
            {
                throw new NullReferenceException();
            }

            _commands.Add(name, command);
            return _commands.ContainsKey(name);
        }

        public bool Remove(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException();
            }
            _commands.Remove(name);
            return !_commands.ContainsKey(name);
        }

        public Command Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException();
            }
            return _commands.FirstOrDefault(entry => entry.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase)).Value;
        }

        public bool Contains(string name)
        {
            return _commands.Any(entry => entry.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
