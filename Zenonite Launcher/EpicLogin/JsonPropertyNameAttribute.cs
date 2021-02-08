using System;

namespace Zenonite_Launcher.Epic_Shit
{
    internal class JsonPropertyNameAttribute : Attribute
    {
        private string v;

        public JsonPropertyNameAttribute(string v)
        {
            this.v = v;
        }
    }
}