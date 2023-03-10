using System.Diagnostics;
using System.Collections.Generic;


namespace Albuquerque.PortKnocking
{
    class PortKnocker
    {
        private int period;
        private List<int> sequence;
        private int sequenceIndex;
        private Stopwatch firstKnockTimeWatch;

        public PortKnocker(List<int> sequence, int period)
        {
            this.period = period;
            this.sequence = sequence;
            this.sequenceIndex = 0;
        }

        public bool Check(int port)
        {
            if (this.sequenceIndex == 0 && port == this.sequence[this.sequenceIndex])
            {   
                this.firstKnockTimeWatch = System.Diagnostics.Stopwatch.StartNew();
                this.sequenceIndex = 1;
                return false;
            }
            else if (port == this.sequence[this.sequenceIndex] &&  firstKnockTimeWatch.ElapsedMilliseconds / 1000.0 <  this.period )
            {
                this.sequenceIndex += 1;

                if (this.sequenceIndex >= this.sequence.Count)
                {
                    this.sequenceIndex = 0;
                    return true;
                }
                return false;
            }
            this.sequenceIndex = 0;
            return false;   
        }
    }
}
