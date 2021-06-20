namespace DotnetCampusP2PFileShare.SDK.Context
{
    public class ProcessReport
    {
        public string Id { set; get; }

        //public DateTime LastUpdateTime { get; set; }

        public double Process { get; set; }

        public double MaxProcess { set; get; } = 100;

        public string Remark { get; set; }
    }
}