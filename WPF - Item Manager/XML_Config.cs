using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Trainingyaes
{
    [Serializable]
    public class XML_Config
    {
        public double WindowHeight;
        public double WindowWidth;
        //Set path
        public string Directory;
    }
}
