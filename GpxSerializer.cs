using System;
using System.Collections.Generic;
using System.Xml;

using kimandtodd.DG200CSharp;
using kimandtodd.DG200CSharp.commands;
using kimandtodd.DG200CSharp.commands.exceptions;
using kimandtodd.DG200CSharp.commandresults;
using kimandtodd.DG200CSharp.commandresults.resultitems;

namespace kimandtodd.GPX_Reader
{
    public class GpxSerializer
    {
        private string _outputPath;
        private ISet<TrackHeaderEntry> _trackHeaderEntries;
        private ISet<IDGTrackPoint> _waypoints;
        private DG200SerialConnection _dgSerialConnection;


        public GpxSerializer()
        {
            this._outputPath = "";
            this._trackHeaderEntries = new HashSet<TrackHeaderEntry>();
            this._waypoints = new HashSet<IDGTrackPoint>();
        }

        public void setFilePath(string newPath)
        {
            this._outputPath = newPath;
        }

        public void setSerialConnection(DG200SerialConnection sc)
        {
            this._dgSerialConnection = sc;
        }

        public string getFilePath()
        {
            return this._outputPath;
        }

        public void addTrackHeaderEntry(TrackHeaderEntry newEntry)
        {
            this._trackHeaderEntries.Add(newEntry);
        }

        public ISet<TrackHeaderEntry> getTrackFileEntries()
        {
            return this._trackHeaderEntries;
        }

        public bool serialize()
        {
            string firstTrackHeaderTime = "";
            ISet<GetDGTrackFileCommandResult> tfResults = new HashSet<GetDGTrackFileCommandResult>();

            // open file
            XmlWriter wr = XmlWriter.Create(this._outputPath);

            // write processing instruction and required intro data
            wr.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");

            //<gpx version="1.1" creator="DG-200 ToolBox v1.1.20.237" xmlns="http://www.topografix.com/GPX/1/1" 
            //xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
            //xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd">
            wr.WriteStartElement("gpx", "http://www.topografix.com/GPX/1/1");
            wr.WriteAttributeString("version", "1.1");
            wr.WriteAttributeString("creator", "DG-200");
            wr.WriteAttributeString("xsi", "schemaLocation", 
                "http://www.w3.org/2001/XMLSchema-instance",
                "http://www.topografix.com/GPX/1/1  http://www.topografix.com/GPX/1/1/gpx.xsd");

            foreach(TrackHeaderEntry the in this._trackHeaderEntries)
            {
                if (firstTrackHeaderTime=="")
                {
                    firstTrackHeaderTime = the.DateTimeString;
                }

                foreach(int tfId in the.getTrackIds())
                {
                    tfResults.Add(this.getTrackFile(tfId));
                }
            }

            wr.WriteStartElement("trk");
            wr.WriteElementString("name", firstTrackHeaderTime);
            wr.WriteElementString("src", "DG-200");

            wr.WriteStartElement("trkseg");

            foreach(GetDGTrackFileCommandResult res in tfResults)
            {
                foreach(IDGTrackPoint tp in res.getTrackPoints())
                {
                    this.writeTrackPoint(tp, wr);
                }
            }

            wr.WriteEndElement(); // trkseg
            wr.WriteEndElement(); // trk
            wr.WriteEndElement(); // gpx
            
            

            // loop through entries
            // if a waypoint, copy that entry to the waypoints set
            // else, serialize

            // close trkseg

            // write waypoints

            // write closing tag

            // close file

            wr.Close();


            return true;
        }

        private GetDGTrackFileCommandResult getTrackFile(int tfId)
        {
            // read entries from device
            GetDGTrackFileCommand c = new GetDGTrackFileCommand();
            c.setSerialConnection(this._dgSerialConnection);

            try
            {
                c.setTrackIndex(tfId);
                c.execute();
            }
            catch (CommandException e)
            {
                Console.WriteLine("An exception was thrown when reading the track file: " + e.Message);
            }

            GetDGTrackFileCommandResult res = (GetDGTrackFileCommandResult)c.getLastResult();

            return res;
        }

        private void writeTrackPoint(IDGTrackPoint tp, XmlWriter wr)
        {
            /*
             * TODO: Add check if isValid is false.
            <trkpt lat="48.045925" lon="-121.715811666667">
			    <time>2015-05-31T18:54:51Z</time>
			    <cmt>3.6km/h</cmt>
			    <ele>120</ele> 
		    </trkpt>*/
            int tpType = tp.getTrackFormat();

            wr.WriteStartElement("trkpt");

            Tuple<Int16, Double> coord = tp.getLatitude();
            wr.WriteAttributeString("lat", coord.Item1.ToString() + "." + coord.Item2.ToString());

            coord = tp.getLongitude();
            wr.WriteAttributeString("lon", coord.Item1.ToString() + "." + coord.Item2.ToString());

            wr.WriteElementString("time", tp.getDateTime().ToString());

            if (tpType == BaseDGTrackPoint.FORMAT_POSITION_DATE_SPEED || tpType == BaseDGTrackPoint.FORMAT_POSITION_DATE_SPEED_ALTITUDE)
            {
                wr.WriteElementString("cmt", tp.getSpeed().ToString() + "km/h");
            }

            if (tpType == BaseDGTrackPoint.FORMAT_POSITION_DATE_SPEED_ALTITUDE)
            {
                wr.WriteElementString("ele", tp.getAltitude().ToString());
            }

            wr.WriteEndElement();
        }        
    }
}
