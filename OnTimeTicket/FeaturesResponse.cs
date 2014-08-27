using System.Collections.Generic;

namespace OnTimeTicket
{
    public class FeaturesResponse
    {
        public List<Feature> data { get; set; }
    }

    public class Feature
    {
        public long id { get; set; }
        public string name { get; set; }
        public WorkflowStep workflow_step { get; set; }

        public override string ToString()
        {
            return string.Format("[otf: {0}] {1}", id, name);
        }
    }

    public class WorkflowStep
    {
        public long id { get; set; }
        public string name { get; set; }
    }
}