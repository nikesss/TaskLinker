using ServiceStack;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskTT.ServiceModel
{
    [Route("/parse")]
    public class ParsingdPage : IReturn
    {
        public string Start { get; set; }
    }

}
