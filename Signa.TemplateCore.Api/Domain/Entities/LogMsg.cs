using Signa.TemplateCore.Api.Helpers;
using System;

namespace Signa.TemplateCore.Api.Data.Entities
{
    // TODO: incluir em Signa.Library
    public class LogMsg
    {
        private string _msg01;

        public int TabTipoMsgId { get; set; } = 95;
        public string Msg01
        {
            get { return _msg01; }
            set
            {
                var messages = value.SplitBy(8000, 5);
                _msg01 = messages[0];
                Msg02 = messages[1];
                Msg03 = messages[2];
                Msg04 = messages[3];
                Msg05 = messages[4];
            }
        }
        public string Msg02 { get; set; }
        public string Msg03 { get; set; }
        public string Msg04 { get; set; }
        public string Msg05 { get; set; }

        public int? UsuarioInternetId { get; set; }
        public DateTime DataLog { get; set; }
    }
}
