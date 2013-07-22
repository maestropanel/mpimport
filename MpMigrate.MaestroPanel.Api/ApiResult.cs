namespace MpMigrate.MaestroPanel.Api
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    [XmlRoot("Result")]
    public class ApiResult<T>
    {
        public int StatusCode { get; set; }
        public int ErrorCode { get; set; }        
        public string Message { get; set; }

        public T Details { get; set; }
    }
}
