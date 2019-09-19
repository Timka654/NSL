using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Node.UDPNode
{
    public class UDPChannelIO
    {
        public static string Parse(string Url, out string ObjectUri)
        {
            ObjectUri = null;

            string ChannelUri = null;

            try
            {
                System.Uri ParsedURI = new System.Uri(Url);

                ChannelUri = ParsedURI.Authority;

                ObjectUri = ParsedURI.AbsolutePath;
            }
            catch (Exception)
            {
                ObjectUri = null;

                ChannelUri = null;
            }

            return ChannelUri;
        }

        const ushort HeaderMarker = 0xFFA1;

        const ushort HeaderEndMarker = 0xFFA2;

        public static void PrepareInboundMessage(byte[] Buffer,
            out ITransportHeaders RequestHeaders,
            out Stream RequestStream)
        {
            try
            {
                MemoryStream MS = new MemoryStream(Buffer);
                BinaryReader BR = new BinaryReader(MS);

                RequestHeaders = new TransportHeaders();

                string Uri = BR.ReadString();

                if (Uri != null && Uri != "")
                {
                    string ObjectUri;

                    string ChannelUri = UDPChannelIO.Parse(Uri, out ObjectUri);

                    if (ChannelUri == null)
                    {
                        ObjectUri = Uri;
                    }

                    RequestHeaders[CommonTransportKeys.RequestUri] = ObjectUri;
                }

                // Get the headers
                ushort HM = BR.ReadUInt16();

                if (HM != HeaderMarker && HM != HeaderEndMarker)
                {
                    throw new Exception("Invalid message was received");
                }

                while (HM != HeaderEndMarker)
                {
                    string HeaderName = BR.ReadString();

                    string HeaderValue = BR.ReadString();

                    RequestHeaders[HeaderName] = HeaderValue;

                    // read header marker
                    HM = BR.ReadUInt16();
                }

                int StreamSize = BR.ReadInt32();

                // Get the request stream
                RequestStream = new MemoryStream(Buffer, (int)BR.BaseStream.Position, StreamSize);
            }
            catch (Exception e)
            {
                Console.WriteLine("ProcessIncomingMessage failed with error: " + e.Message);

                RequestHeaders = null;

                RequestStream = null;
            }
        }

        public static void PrepareOutboundMessage(string Uri,
            ITransportHeaders RequestHeaders,
            Stream RequestStream,
            out byte[] Buffer
            )
        {
            try
            {
                // Setup a memory stream to hold the entire message
                MemoryStream MS = new MemoryStream(2048);

                BinaryWriter BW = new BinaryWriter(MS, Encoding.UTF8);

                //
                // Write the headers to the memory buffer
                //
                // Format is:
                //
                // URI string
                // <header marker>
                // header name string
                // header value string
                // <header marker>
                // ...
                // <header end marker>
                // <Request stream size>
                // <Request stream bytes>
                //
                // Write the URI whatever it is

                BW.Write(Uri);

                foreach (DictionaryEntry Header in RequestHeaders)
                {
                    string HeaderName = (string)Header.Key;

                    // skip headers beginning with "__"
                    if ((HeaderName.Length < 2) || ((HeaderName[0] != '_') && (HeaderName[1] != '_')))
                    {
                        BW.Write(HeaderMarker);

                        BW.Write(HeaderName);

                        BW.Write(Header.Value.ToString());
                    }
                }

                BW.Write(HeaderEndMarker);

                const int bufsize = 256;

                byte[] TransferBuffer = new byte[bufsize];

                BW.Write((int)(RequestStream.Length));

                int len;

                do
                {
                    len = RequestStream.Read(TransferBuffer, 0, bufsize);

                    if (len > 0)
                    {
                        BW.Write(TransferBuffer);
                    }
                }
                while (len != 0);

                BW.Flush();

                MS.Position = 0;

                Buffer = MS.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine("PrepareOutgoingMessage failed with error: " + e.Message);

                Buffer = null;
            }
        }
    }
}
