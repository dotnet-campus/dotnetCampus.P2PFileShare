using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetCampusP2PFileShare.Model
{
    public class ResourceModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { set; get; }

        public string ResourceId { set; get; }

        public string ResourceName { set; get; }

        public string LocalPath { set; get; }

        public string ResourceSign { set; get; }

        public string ResourceFileDetail { set; get; }
    }
}