using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Logs;

namespace Dmioks.Common.Utils
{
    public static class ResourceHelper
    {
        private static readonly ILog m_logger = LogFactory.GetLogger(typeof(ResourceHelper));

        public static string FileToString(Assembly assembly, string sNameSpace, string sEmbeddedResourceFileName)
        {
            try
            {
                ExcpHelper.ThrowIf<ArgumentException>(assembly == null, "assembly is null");
                ExcpHelper.ThrowIf<ArgumentException>(sNameSpace == null, "namespace is null");
                ExcpHelper.ThrowIf<ArgumentException>(string.IsNullOrEmpty(sEmbeddedResourceFileName), "EmbeddedResourceFileName is null or empty");

#if DEBUG
                string[] arrNames = assembly.GetManifestResourceNames();
#endif

                using (Stream stream = assembly.GetManifestResourceStream(string.Concat(sNameSpace, '.', sEmbeddedResourceFileName)))
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        string sFile = sr.ReadToEnd();

                        return sFile;
                    }
                }
            }
            catch (Exception e)
            {
                m_logger.Error(e, $"FileToString(assembly={assembly}, namespace={sNameSpace}, filename={sEmbeddedResourceFileName}) ERROR.");
            }

            return null;
        }
    }
}
