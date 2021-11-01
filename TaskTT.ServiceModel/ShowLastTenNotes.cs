using ServiceStack;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskTT.ServiceModel
{
    [Route("/parse")]
    [Route("/parse/{PageParse}")]
    public class ShowLastTenNotes : IReturn<HShowLastTenNotesResponse>
    {
        public string PageParse { get; set; }
    }
    public class HShowLastTenNotesResponse
    {
        public string Result { get; set; }
    }
}
