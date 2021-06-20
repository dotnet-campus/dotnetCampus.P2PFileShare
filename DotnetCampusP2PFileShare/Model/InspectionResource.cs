namespace DotnetCampusP2PFileShare.Model
{
    /// <summary>
    /// 准备寻找的资源信息
    /// </summary>
    public class InspectionResource
    {
        /// <summary>
        /// 资源的标识
        /// </summary>
        public string ResourceId { set; get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ResourceId}";
        }
    }
}