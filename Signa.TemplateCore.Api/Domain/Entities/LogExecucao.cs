using System;
using System.Xml;

namespace Signa.TemplateCore.Api.Data.Entities
{
    public class LogExecucao
    {
        public DateTime DataExecucao { get; set; }
        public int? FuncaoId { get; set; }
        public XmlDocument ParametroXmlIn { get; set; }
        public XmlDocument ParametroXmlOut { get; set; }
        public int TabTipoMsgId { get; set; }
    }
}
