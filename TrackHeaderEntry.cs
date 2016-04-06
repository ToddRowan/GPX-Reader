using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using kimandtodd.DG200CSharp.commandresults.resultitems;

namespace kimandtodd.GPX_Reader
{
    public class TrackHeaderEntry
    {

        public string Date
        {
            get
            {
                return this._firstHeaderDateTime.ToString(); // Need to decide on formatting. 
            }
        }
        public int TrackfileCount
        {
            get
            {
                return this._trackIds.Count;
            }
        }
            

        private HashSet<int> _trackIds;
        private DateTime _firstHeaderDateTime;

        public TrackHeaderEntry(DGTrackHeader th)
        {
            this._trackIds = new HashSet<int>();

            this._trackIds.Add(th.getFileIndex());

            this._firstHeaderDateTime = this.makeDateTime(th); 
        }

        private DateTime makeDateTime(DGTrackHeader th)
        {
            int year = th.getYear() + (th.getYear() == 80 ? 1900 : 2000);

            return new DateTime(year, th.getMonth(), th.getDay(),
                    th.getHour(), th.getMinute(), th.getSeconds(), DateTimeKind.Utc);
        }

        public void addTrackId(int id)
        {
            this._trackIds.Add(id);
        }
    }
}
