using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetCampusP2PFileShareTracer
{
    public class Node
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        public string MainIp { set; get; }

        public string LocalIp { set; get; }

        public DateTime LastUpdate { set; get; }
    }
}