using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskTT.ServiceModel.Types
{
    public class Entitis
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string TypeEntitis { get; set; }
        public string ValueEntitis { get; set; }

        [ForeignKey(typeof(ParsedPage), OnDelete = "CASCADE")]
        public int ParsedPageId { get; set; }
    }
}
