namespace DotnetCampusP2PFileShare.Model
{
    public class InspectionResource
    {
        public string ResourceId { set; get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ResourceId}";
        }
    }
}