using System;

namespace qbook.Scripting
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AutocompleteParamInfoAttribute : Attribute
    {

        // Provides name of the member
        private string name;

        // Provides description of the member
        private string action;

        // Constructor
        public AutocompleteParamInfoAttribute(string name, string action)
        {
            this.name = name;
            this.action = action;
        }

        // property to get name
        public string Name
        {
            get { return name; }
        }

        // property to get description
        public string Action
        {
            get { return action; }
        }
    }
}
